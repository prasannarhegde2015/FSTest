using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace ParseNotification
{
    class WindowsRecoveryScenario
    {
        public void ClientCode()
        {
            ClsWindowsErrrorHandler myItem = new ClsWindowsErrrorHandler();
            // we need to add the delegate event to new object
            myItem.OnPopUpWindowOccurred += new ClsWindowsErrrorHandler.PopupHandler(UIAHandleWindowsErrorEvent);
            Thread t2 = new Thread(delegate ()
            {
                CheckAsync(myItem);
            });
            t2.Start();
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine("Exeucting line " + i);
                Thread.Sleep(1000);
            }
            Console.Read();
        }

        private void UIAHandleWindowsErrorEvent(object a, ShipArgs e)
        {
            Console.WriteLine(e.Message);
            ClsWindowsErrrorHandler myItem2 = new ClsWindowsErrrorHandler();
            myItem2.ClosePopUp();
        }

        static void CheckAsync(ClsWindowsErrrorHandler spp)
        { 
            bool ck = true;
            while (ck == true)
            {
                spp.ErrorOccured = spp.IswindowExists() ? "Yes" : "No";
            }
        }

    }

    public class ShipArgs : EventArgs
    {
        private string message;
        public ShipArgs(string message)
        {
            this.message = message;
        }
        // This is a straightforward implementation for
        // declaring a public field
        public string Message
        {
            get
            {
                return message;
            }
        }
    }

    public class ClsWindowsErrrorHandler

    {
        private string _errorOccured;
        // The delegate procedure we are assigning to our object
        public delegate void PopupHandler(object myObject, ShipArgs myArgs);
        public event PopupHandler OnPopUpWindowOccurred;
        public string ErrorOccured
        {
            set
            {
                this._errorOccured = value;
                // We need to check whether a tracking number
                // was assigned to the field
                if (this._errorOccured.ToLower() == "yes")
                {
                    ShipArgs myArgs = new ShipArgs("Popup has occured");
                    // errorOccured is 'yes', raise the event.
                    OnPopUpWindowOccurred(this, myArgs);
                }
                else
                {

                }

            }

        }
        // ******  Constructor for Class
        public ClsWindowsErrrorHandler()
        {

        }

        public void ClosePopUp()
        {
            AutomationElement ae = AutomationElement.RootElement;
            Condition cond1 = new AndCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
                new PropertyCondition(AutomationElement.NameProperty, "LOWIS: Connect"));
            AutomationElement win = ae.FindFirst(TreeScope.Descendants, cond1);
            if (win != null) // perform the action when window is found
            {
                Console.WriteLine(" Hearing to Event fired I Got Window ...... ");
                Condition cond2 = new AndCondition(
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
                    new PropertyCondition(AutomationElement.NameProperty, "Close"));
                AutomationElement btn = win.FindFirst(TreeScope.Descendants, cond2);
                InvokePattern ivk = (InvokePattern)btn.GetCurrentPattern(InvokePattern.Pattern);
                ivk.Invoke();
            }
            else
            {
                Console.WriteLine(" Hearing to Event fired I did not get a Window ...... ");
                return;
            }
        }

        public bool IswindowExists()
        {
            AutomationElement ae = AutomationElement.RootElement;
            Condition cond1 = new AndCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
                new PropertyCondition(AutomationElement.NameProperty, "LOWIS: Connect"));
            AutomationElement win = ae.FindFirst(TreeScope.Descendants, cond1);
            bool winexisits = (win != null) ? true : false;
            return winexisits;
        }

    }
}
