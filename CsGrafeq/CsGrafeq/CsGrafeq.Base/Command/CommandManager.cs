namespace CsGrafeq.Command;

public class CommandManager
{
    private readonly LinkedList<CommandBase> _doList;
    private LinkedListNode<CommandBase> _currentDoNode;

    public CommandManager()
    {
        _doList = new([new CommandBase(null,null,null, null, null)]);
        _currentDoNode = _doList.First!;
    }

    public void ReDo()
    {
        if (_currentDoNode != _doList.Last)
        {
            _currentDoNode = _currentDoNode.Next!;
            _currentDoNode.Value.Do.Invoke(_currentDoNode.Value.Tag);
            
            Console.WriteLine("Redo:"+_currentDoNode.Value.Description);
        }
    }

    public void UnDo()
    {
        if (_currentDoNode != _doList.First)
        {
            _currentDoNode.Value.UnDo.Invoke(_currentDoNode.Value.Tag);
            Console.WriteLine("Undo:"+_currentDoNode.Value.Description);
            _currentDoNode = _currentDoNode.Previous!;
        }
    }

    public void Do(object? tag,Action<object?> initFunc, Action<object?> doFunc, Action<object?> unDoFunc, Action<object?> clearFunc, bool invokeDo = false,string des="")
    {
        while (_doList.Last != _currentDoNode)
        {
            _doList.Last?.Value?.Clear(_doList.Last.Value.Tag);
            _doList.RemoveLast();
        }

        initFunc(tag);
        if (invokeDo) doFunc.Invoke(tag);
        _doList.AddLast(new CommandBase(tag,initFunc, doFunc, unDoFunc, clearFunc){Description = des});
        _currentDoNode = _doList.Last;
    }

    public void Do(CommandBase command, bool invokeDo = false)
    {
        while (_doList.Last != _currentDoNode)
        {
            _doList.Last!.Value.Clear(_doList.Last.Value.Tag);
            _doList.RemoveLast();
        }
        command.Init.Invoke(command.Tag);
        if (invokeDo) command.Do.Invoke(command.Tag);
        _doList.AddLast(command);
        _currentDoNode = _doList.Last;
    }
}