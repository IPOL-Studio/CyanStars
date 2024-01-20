using MunNovel.Command;

namespace MunNovel.Metadata
{
    public interface ICommandParameterAccessor
    {
        void SetValue(ICommand command, object value);
        object GetValue(ICommand command);
    }

    public interface ICommandParameterAccessor<TCommand, TParameter> : ICommandParameterAccessor
        where TCommand : ICommand
    {
        TParameter GetValue(TCommand command);
        void SetValue(TCommand command, TParameter value);
    }

    public abstract class CommandParameterAccessorBase<TCommand, TParameter> : ICommandParameterAccessor<TCommand, TParameter>
        where TCommand : ICommand
    {
        object ICommandParameterAccessor.GetValue(ICommand command) =>
            GetValue((TCommand)command);

        void ICommandParameterAccessor.SetValue(ICommand command, object value) =>
            SetValue((TCommand)command, (TParameter)value);

        public abstract TParameter GetValue(TCommand command);
        public abstract void SetValue(TCommand command, TParameter value);
    }
}