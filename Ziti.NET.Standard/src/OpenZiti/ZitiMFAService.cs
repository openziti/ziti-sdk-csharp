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

	public class ZitiMFAService
	{
		/*public ZitiIdentity ZID { get; set; }

		public ZitiMFAActions(ZitiIdentity ziti_identity)
		{
			this.ZID = ziti_identity;
		}*/

		private static void on_submit_mfa(IntPtr ziti_context, int status, IntPtr ctx)
		{
			//ZitiIdentity.TunnelCB cb = Marshal.PtrToStructure<ZitiIdentity.TunnelCB>(ctx);
			ZitiIdentity.TunnelCB.ZitiResponseDelegate cb = Marshal.GetDelegateForFunctionPointer<ZitiIdentity.TunnelCB.ZitiResponseDelegate>(ctx);

			ZitiMFAStatusEvent evt = new ZitiMFAStatusEvent()
			{
				status	= (ZitiStatus)status,
				//id		= ZID,
				operationType = MFAOperationType.MFA_AUTH_STATUS				
			};
			cb?.Invoke(evt);

			//cb.ZitiResponse(evt);
			//ZID.InitOpts.ZitiMFAStatusEvent(evt);
		}

		public static void submit_mfa(ZitiContext context, string code, IntPtr status_ctx)
		{
			OpenZiti.Native.API.ziti_mfa_auth(context.nativeZitiContext, code, on_submit_mfa, status_ctx);
		}

		private static void on_enable_mfa(IntPtr ziti_context, int status, IntPtr /* ziti_mfa_enrollment*/ enrollment, IntPtr ctx)
		{
			OpenZiti.Native.ziti_mfa_enrollment ziti_mfa_enrollment = Marshal.PtrToStructure<OpenZiti.Native.ziti_mfa_enrollment>(enrollment);
			// ZitiIdentity.TunnelCB cb = Marshal.PtrToStructure<ZitiIdentity.TunnelCB>(ctx);
			ZitiIdentity.TunnelCB.ZitiResponseDelegate cb = Marshal.GetDelegateForFunctionPointer<ZitiIdentity.TunnelCB.ZitiResponseDelegate>(ctx);

			ZitiMFAStatusEvent evt = new ZitiMFAStatusEvent()
			{
				status = (ZitiStatus)status,
				//id = ZID,
				operationType = MFAOperationType.ENROLLMENT_CHALLENGE,
				provisioningUrl = ziti_mfa_enrollment.provisioning_url,
				// recoveryCodes = ziti_mfa_enrollment.recovery_codes,
			};
			cb?.Invoke(evt);
			// cb.ZitiResponse(evt);
			//ZID.InitOpts.ZitiMFAStatusEvent(evt);
			// get TunnelCB from IntPtr ctx
		}

		public static void ziti_mfa_enroll(ZitiContext context, IntPtr status_ctx)
		{
			OpenZiti.Native.API.ziti_mfa_enroll(context.nativeZitiContext, on_enable_mfa, status_ctx);
		}

		private static void on_verify_mfa(IntPtr ziti_context, int status, IntPtr ctx)
		{
			//ZitiIdentity.TunnelCB cb = Marshal.PtrToStructure<ZitiIdentity.TunnelCB>(ctx);
			ZitiIdentity.TunnelCB.ZitiResponseDelegate cb = Marshal.GetDelegateForFunctionPointer<ZitiIdentity.TunnelCB.ZitiResponseDelegate>(ctx);

			ZitiMFAStatusEvent evt = new ZitiMFAStatusEvent()
			{
				status = (ZitiStatus)status,
				//id = ZID,
				operationType = MFAOperationType.ENROLLMENT_VERIFICATION
			};
			cb?.Invoke(evt);
			//ZID.InitOpts.ZitiMFAStatusEvent(evt);
		}

		public static void verify_mfa(ZitiContext context, string code, IntPtr status_ctx)
		{
			OpenZiti.Native.API.ziti_mfa_verify(context.nativeZitiContext, code, on_verify_mfa, status_ctx);
		}

		private static void on_remove_mfa(IntPtr ziti_context, int status, IntPtr ctx)
		{
			// ZitiIdentity.TunnelCB cb = Marshal.PtrToStructure<ZitiIdentity.TunnelCB>(ctx);
			ZitiIdentity.TunnelCB.ZitiResponseDelegate cb = Marshal.GetDelegateForFunctionPointer<ZitiIdentity.TunnelCB.ZitiResponseDelegate>(ctx);

			ZitiMFAStatusEvent evt = new ZitiMFAStatusEvent()
			{
				status = (ZitiStatus)status,
				//id = ZID,
				operationType = MFAOperationType.ENROLLMENT_REMOVE
			};
			cb?.Invoke(evt);
			// cb.ZitiResponse(evt);
			//ZID.InitOpts.ZitiMFAStatusEvent(evt);
		}

		public static void remove_mfa(ZitiContext context, string code, IntPtr status_ctx)
		{
			OpenZiti.Native.API.ziti_mfa_remove(context.nativeZitiContext, code, on_remove_mfa, status_ctx);
		}
	}
}
