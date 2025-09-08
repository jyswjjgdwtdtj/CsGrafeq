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
        public readonly object Tag;
        public readonly Action<object> Do;
        public readonly Action<object> UnDo;
        public readonly Action<object> Clear;
        public Command(object tag, Action<object> doFunc, Action<object> unDoFunc, Action<object> clearFunc)
        {
            Tag = tag;
            Do = doFunc;
            UnDo = unDoFunc;
            Clear = clearFunc;
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
    public void Do(object tag, Action<object> doFunc, Action<object> unDoFunc, Action<object> clearFunc,bool invokeDo=false)
    {
        while (DoList.Last != CurrentDoNode)
        {
            DoList.Last.Value.Clear(DoList.Last.Value.Tag);
            DoList.RemoveLast();
        }
        if(invokeDo) doFunc.Invoke(tag);
        DoList.AddLast(new Command(tag,doFunc,unDoFunc,clearFunc));
        CurrentDoNode = DoList.Last;
    }

}
