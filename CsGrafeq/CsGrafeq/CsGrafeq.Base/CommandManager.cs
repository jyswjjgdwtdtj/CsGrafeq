using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CsGrafeq;
public class CommandManager
{
    private LinkedList<Command> DoList;
    private LinkedListNode<Command> CurrentDoNode;
    public CommandManager() {
        DoList = new LinkedList<Command>([new Command(null,null,null,null)]);
        CurrentDoNode = DoList.First;
    }
    public class Command
    {
        public object Tag { get; protected set; }
        public  Action<object> Do { get; protected set; }
        public  Action<object> UnDo { get; protected set; }
        public  Action<object> Clear { get; protected set; }
        public Command(object tag, Action<object> doFunc, Action<object> unDoFunc, Action<object> clearFunc)
        {
            Tag = tag;
            Do = doFunc;
            UnDo = unDoFunc;
            Clear = clearFunc;
        }
        public void SetTag(object tag)
        {
            Tag = tag;
        }
        public void SetDo<T>(Action<T> doFunc)
        {
            Do = (arg) => { doFunc((T)arg); };
        }
        public void SetUnDo<T>(Action<T> unDoFunc)
        {
            UnDo = (arg) => { unDoFunc((T)arg); };
        }
        public void SetClear<T>(Action<T> clearFunc)
        {
            Clear = (arg) => { clearFunc((T)arg); };
        }
    }
    public void ReDo()
    {
        if (CurrentDoNode != DoList.Last)
        {
            CurrentDoNode = CurrentDoNode.Next;
            CurrentDoNode.Value.Do.Invoke(CurrentDoNode.Value.Tag);
        }
    }

    public void UnDo()
    {
        if (CurrentDoNode != DoList.First)
        {
            CurrentDoNode.Value.UnDo.Invoke(CurrentDoNode.Value.Tag);
            CurrentDoNode = CurrentDoNode.Previous;
        }
    }
    public void Do<T>(T tag, Action<T> doFunc, Action<T> unDoFunc, Action<T> clearFunc,bool invokeDo=false)
    {
        while (DoList.Last != CurrentDoNode)
        {
            DoList.Last.Value.Clear(DoList.Last.Value.Tag);
            DoList.RemoveLast();
        }
        if(invokeDo) doFunc.Invoke(tag);
        DoList.AddLast(new Command((object)tag,(arg)=> { doFunc((T)arg); }, (arg) => { unDoFunc((T)arg); }, (arg) => { clearFunc((T)arg); }));
        CurrentDoNode = DoList.Last;
    }
    public void Do(Command command,bool invokeDo=false)
    {
        while (DoList.Last != CurrentDoNode)
        {
            DoList.Last.Value.Clear(DoList.Last.Value.Tag);
            DoList.RemoveLast();
        }
        if (invokeDo) command.Do.Invoke(command.Tag);
        DoList.AddLast(command);
        CurrentDoNode = DoList.Last;
    }

}
