using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Safali
{
    public class PWBox : INotifyPropertyChanged
    {
        public string Password
        {
            get => new string('●', PlainPassword.Length);
            set
            {
                if (value.Length < PlainPassword.Length)
                {
                    PlainPassword = PlainPassword.Substring(0, value.Length);
                }
                else if (value.Length > PlainPassword.Length)
                {
                    PlainPassword += value.Substring(PlainPassword.Length, value.Length - PlainPassword.Length);
                }
                else
                {
                    PlainPassword = value;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Password)));
            }
        }

        public string PlainPassword
        {
            get => plainPassword;
            set
            {
                plainPassword = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlainPassword)));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        string plainPassword
        {
            get => Marshal.PtrToStringUni(Marshal.SecureStringToGlobalAllocUnicode(cipher));
            set
            {
                cipher.Clear();

                foreach (var ch in value)
                {
                    cipher.AppendChar(ch);
                }
            }
        }
        SecureString cipher = new SecureString();
    }
}
