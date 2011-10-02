using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace KinectClient
{
    public class Console
    {
        private static object lockObj = new Object();
        private static Console instance = null;

        private TextBox output;

        private Console(TextBox textControl)
        {
            output = textControl;
        }

        public static void createInstance(TextBox textBox)
        {
            if (null != instance)
            {
                return;
            }
            lock (lockObj)
            {
                instance = new Console(textBox);
            }
        }

        public static Console Instance
        {
            get
            {
                if (null == instance)
                {
                    throw new NoConsoleInstanceException("Console instance is not created.");
                }
                lock (lockObj)
                {
                    return instance;
                }
            }
        }

        private void AppendText(string message)
        {
            output.Dispatcher.Invoke(new Action(delegate
            {
                output.Text += message;
                output.ScrollToEnd();
            }));
        }

        public void Clear()
        {
            output.Dispatcher.Invoke(new Action(delegate
            {
                output.Clear();
            }));
        }

        public void Write<T>(T value) where T : struct
        {
            AppendText(Convert.ToString(value));
        }

        public void Write(Exception ex)
        {
            AppendText(ex.ToString());
        }

        public void Write(string format, params object[] args)
        {
            AppendText(string.Format(format, args));
        }

        public void WriteLine<T>(T value) where T : struct
        {
            AppendText(Convert.ToString(value) + System.Environment.NewLine);
        }

        public void WriteLine(Exception ex)
        {
            AppendText(ex.ToString() + System.Environment.NewLine);
        }

        public void WriteLine(string format, params object[] args)
        {
            AppendText(string.Format(format, args) + System.Environment.NewLine);
        }
    }

    public class NoConsoleInstanceException : ApplicationException
    {
        public NoConsoleInstanceException()
            : base()
        {
        }

        public NoConsoleInstanceException(string message)
            : base(message)
        {
        }

        public NoConsoleInstanceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
