using System;
using System.Windows.Input;

namespace ImageRecognition.Utility
{
	public class RelayCommand<T> : ICommand
	{
		private Action<T> execute;
		private bool canExecute = true;

		public RelayCommand(Action<T> execute)
		{
			this.execute = execute;
		}

		public event EventHandler CanExecuteChanged = null;

		public bool CanExecute
		{
			get { return canExecute; }
			set
			{
				if (canExecute == value)
					return;
				canExecute = value;
				CanExecuteChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		bool ICommand.CanExecute(object parameter)
		{
			return this.CanExecute;
		}

		public void Execute(object parameter)
		{
			if (!(parameter is T))
				execute((T)parameter);
		}
	}

	public class RelayCommand : ICommand
	{
		private Action execute;
		private bool canExecute = true;

		public RelayCommand(Action execute)
		{
			this.execute = execute;
		}

		public event EventHandler CanExecuteChanged = null;

		bool ICommand.CanExecute(object parameter)
		{
			return canExecute;
		}

		public bool CanExecute
		{
			get { return canExecute; }
			set
			{
				if (canExecute == value)
					return;
				canExecute = value;
				CanExecuteChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public void Execute(object parameter)
		{
			execute();
		}
	}

}