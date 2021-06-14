using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;

namespace WPFHelpers.Helpers
{
    public class CommandHelper : ObservableObject
    {

        private ICommand command;
        public ICommand Command => CreateCommand();

        private Action execute;

        public CommandHelper(Action execute)
        {
            this.execute = execute;
        }

        public ICommand CreateCommand()
        {
            if (command == null)
            {
                command = new RelayCommand(execute, () => true);
                return command;
            }
            else
            {
                return command;
            }
        }
    }

    public class CommandHelper<T> : ObservableObject
    {

        private ICommand command;
        public ICommand Command => CreateCommand();

        private Action<T> execute;

        public CommandHelper(Action<T> execute)
        {
            this.execute = execute;
        }

        public ICommand CreateCommand()
        {
            if (command == null)
            {
                command = new RelayCommand<T>(p => execute(p), p => true);
                return command;
            }
            else
            {
                return command;
            }
        }
    }
}
