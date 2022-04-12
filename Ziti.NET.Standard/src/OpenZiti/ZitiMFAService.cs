using System;
using System.Runtime.InteropServices;

namespace OpenZiti
{
	public enum MFAOperationType
	{
		MFA_AUTH_STATUS,
		ENROLLMENT_VERIFICATION,
		ENROLLMENT_REMOVE,
		ENROLLMENT_CHALLENGE
	}

	public struct ZitiMFAEnrollment
	{
		public bool isVerified;
		public string[] recoveryCodes;
		public string provisioningUrl;
	}

	public class ZitiMFAService
	{
		private static void on_submit_mfa(IntPtr ziti_context, int status, IntPtr ctx)
		{
			ZitiIdentity.TunnelCB.ZitiResponseDelegate cb = Marshal.GetDelegateForFunctionPointer<ZitiIdentity.TunnelCB.ZitiResponseDelegate>(ctx);

			ZitiMFAStatusEvent evt = new ZitiMFAStatusEvent()
			{
				status	= (ZitiStatus)status,
				operationType = MFAOperationType.MFA_AUTH_STATUS				
			};
			cb?.Invoke(evt);
		}

		public static void submit_mfa(ZitiIdentity zid, string code, IntPtr status_ctx)
		{
			OpenZiti.Native.API.ziti_mfa_auth(zid.WrappedContext.nativeZitiContext, code, on_submit_mfa, status_ctx);
		}

		private static void on_enable_mfa(IntPtr ziti_context, int status, IntPtr /*ziti_mfa_enrollment*/ enrollment, IntPtr ctx)
		{
			OpenZiti.Native.ziti_mfa_enrollment ziti_mfa_enrollment = Marshal.PtrToStructure<OpenZiti.Native.ziti_mfa_enrollment>(enrollment);
			ZitiIdentity.TunnelCB.ZitiResponseDelegate cb = Marshal.GetDelegateForFunctionPointer<ZitiIdentity.TunnelCB.ZitiResponseDelegate>(ctx);

			ZitiMFAStatusEvent evt = new ZitiMFAStatusEvent()
			{
				status = (ZitiStatus)status,
				isVerified = ziti_mfa_enrollment.is_verified,
				operationType = MFAOperationType.ENROLLMENT_CHALLENGE,
				provisioningUrl = ziti_mfa_enrollment.provisioning_url,
			};

			if (ziti_mfa_enrollment.recovery_codes != IntPtr.Zero)
			{
				// Could not fetch the size of the array from the intptr
				IntPtr[] recoveryCodePointers = new IntPtr[20];
				Marshal.Copy(ziti_mfa_enrollment.recovery_codes, recoveryCodePointers, 0, 20);
				evt.recoveryCodes = new string[20];

				for (int i = 0; i < 20; i++)
				{
					string value = Marshal.PtrToStringAnsi(recoveryCodePointers[i]);
					evt.recoveryCodes[i] = value;
				}
			}
			
			cb?.Invoke(evt);

		}

		public static void ziti_mfa_enroll(ZitiIdentity zid, IntPtr status_ctx)
		{
			OpenZiti.Native.API.ziti_mfa_enroll(zid.WrappedContext.nativeZitiContext, on_enable_mfa, status_ctx);
		}

		private static void on_verify_mfa(IntPtr ziti_context, int status, IntPtr ctx)
		{
			ZitiIdentity.TunnelCB.ZitiResponseDelegate cb = Marshal.GetDelegateForFunctionPointer<ZitiIdentity.TunnelCB.ZitiResponseDelegate>(ctx);

			ZitiMFAStatusEvent evt = new ZitiMFAStatusEvent()
			{
				status = (ZitiStatus)status,
				operationType = MFAOperationType.ENROLLMENT_VERIFICATION
			};
			cb?.Invoke(evt);
		}

		public static void verify_mfa(ZitiIdentity zid, string code, IntPtr status_ctx)
		{
			OpenZiti.Native.API.ziti_mfa_verify(zid.WrappedContext.nativeZitiContext, code, on_verify_mfa, status_ctx);
		}

		private static void on_remove_mfa(IntPtr ziti_context, int status, IntPtr ctx)
		{
			ZitiIdentity.TunnelCB.ZitiResponseDelegate cb = Marshal.GetDelegateForFunctionPointer<ZitiIdentity.TunnelCB.ZitiResponseDelegate>(ctx);

			ZitiMFAStatusEvent evt = new ZitiMFAStatusEvent()
			{
				status = (ZitiStatus)status,
				operationType = MFAOperationType.ENROLLMENT_REMOVE
			};
			cb?.Invoke(evt);
		}

		public static void remove_mfa(ZitiIdentity zid, string code, IntPtr status_ctx)
		{
			OpenZiti.Native.API.ziti_mfa_remove(zid.WrappedContext.nativeZitiContext, code, on_remove_mfa, status_ctx);
		}
	}
}
