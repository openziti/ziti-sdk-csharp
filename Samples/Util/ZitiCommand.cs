using System;
using System.Collections.Generic;
using System.Text;

namespace OpenZiti {
    public class ZitiCommand {
        
        public struct NextAction {
            public int command;

        }

        public struct Result {
            public string id;
            public MFAOperationType operationType;
            public IntPtr result;
            public int status;
        }

        public struct Options {
            internal event EventHandler<NextAction> OnNextAction;
            internal void NextAction(NextAction command) {
                OnNextAction?.Invoke(this, command);

            }

            public void InvokeNextCommand(int[] supportedCmds) {
                NextAction action = new ZitiCommand.NextAction() {
                    command = ZitiCommand.GetNextCommand(supportedCmds)
                };
                this.NextAction(action);
            }
        }

        public static void isSupported(int[] supportedCmds, int index, string statement) {           
            if (checkSupported(supportedCmds, index)) {
                Console.WriteLine("Select {0} to {1}", index, statement);
            }
        }

        public static bool checkSupported(int[] supportedCmds, int index) {
            return Array.Exists(supportedCmds, x => x == index);
        }

        public static int GetNextCommand(int[] supportedCmds) {
            int choice = -1;

            do {
                Console.WriteLine("Choose one of the tunnel options: ");
                isSupported(supportedCmds, 1, "Dial service");
                isSupported(supportedCmds, 2, "Enable MFA for the identity");
                isSupported(supportedCmds, 3, "Verify MFA for the identity");
                isSupported(supportedCmds, 4, "Remove MFA for the identity");
                isSupported(supportedCmds, 5, "Submit MFA for the identity");
                isSupported(supportedCmds, 6, "Get MFA recovery codes for the identity");
                isSupported(supportedCmds, 7, "Generate MFA recovery codes for the identity");
                isSupported(supportedCmds, 8, "Enable identity");
                isSupported(supportedCmds, 9, "Disable identity");
                isSupported(supportedCmds, 10, "Check identity enabled status");
                isSupported(supportedCmds, 11, "Invoke Endpoint satus change");
                isSupported(supportedCmds, 0, "Exit from the application");
                Console.WriteLine("Enter your choice and press enter: ");
                string value = Console.ReadLine();
                try {
                    choice = Convert.ToInt32(value);
                    if (!checkSupported(supportedCmds, choice)) {
                        throw new Exception("wrong option");
                    }

                } catch (Exception e) {
                    Console.WriteLine("You have entered a wrong value {0}, try again (Y/N) : ", value);
                    string retryVar = Console.ReadLine();
                    if (!("Y".Equals(retryVar) || "y".Equals(retryVar))) {
                        return 0; // exit code
                    }
                }
            } while (!checkSupported(supportedCmds, choice));

            return choice;
        }

    }
}
