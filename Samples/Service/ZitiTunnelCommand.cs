using System;
using System.Collections.Generic;
using System.Text;

namespace OpenZiti
{
	public class ZitiTunnelCommand
	{
		public struct NextAction
		{
			public int command;

		}

		public struct TunnelResult
		{
			public string id;
			public MFAOperationType operationType;
			public IntPtr result;
			public int status;
		}

		public struct Options
		{
			internal event EventHandler<NextAction> OnNextAction;
			internal event EventHandler<TunnelResult> OnTunnelResult;
			internal void NextAction(NextAction command)
			{
				OnNextAction?.Invoke(this, command);

			}

			internal void TunnelResult(TunnelResult result)
			{
				OnTunnelResult?.Invoke(this, result);
			}
			public void InvokeNextTunnelCommand()
			{
				NextAction action = new ZitiTunnelCommand.NextAction()
				{
					command = ZitiTunnelCommand.GetNextTunnelCommand()
				};
				this.NextAction(action);
			}
		}

		public static int GetNextTunnelCommand()
		{
			int choice = -1;

			do
			{
				Console.WriteLine("Choose one of the tunnel options: ");
				Console.WriteLine("Enable MFA for the identity: 1");
				Console.WriteLine("Verify MFA for the identity: 2");
				Console.WriteLine("Remove MFA for the identity: 3");
				Console.WriteLine("Submit MFA for the identity: 4");
				Console.WriteLine("Dial service: 5");
				Console.WriteLine("Exit from the application: 0");
				Console.WriteLine("Enter your choice and press enter: ");
				string value = Console.ReadLine();
				try
				{
					choice = Convert.ToInt32(value);

				}
				catch (Exception e)
				{
					Console.WriteLine("You have entered a wrong value {0}, try again (Y/N) : ", value);
					string retryVar = Console.ReadLine();
					if (!("Y".Equals(retryVar) || "y".Equals(retryVar)))
					{
						return 0; // exit code
					}
				}
			} while (choice == -1 || choice > 5);
			
			return choice;
		}

	}
}
