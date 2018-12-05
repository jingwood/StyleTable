/*****************************************************************************
 * 
 * ActionManager
 * 
 * - Common undo/redo framework, help to manage Actions for .NET Application.
 *   
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
 * PURPOSE.
 *
 * Copyright (c) 2012-2013 unvell.com, all rights reserved.
 * Copyright (c) 2012-2018 Jingwood, all rights reserved.
 *
 ****************************************************************************/


using System;
using System.Linq;
using System.Collections.Generic;

namespace Jingwood.WindowsFormControl.StyleTable.Common
{
	public sealed class ActionManager
	{
		private static readonly string LOGKEY = "actionmanager";

		private ActionManager() { }

		public ActionManager(object obj)
		{
			if (!instancesForObject.ContainsKey(obj))
			{
				instancesForObject.Add(obj, this);
			}
		}

		private static Dictionary<object, ActionManager> instancesForObject = new Dictionary<object, ActionManager>();

		public static ActionManager InstanceForObject(object obj)
		{
			ActionManager instance;
			if (!instancesForObject.TryGetValue(obj, out instance))
			{
				instance = new ActionManager();
				instancesForObject.Add(obj, instance);
			}
			return instance;
		}

		private List<IUndoableAction> undoStack = new List<IUndoableAction>();
		private Stack<IUndoableAction> redoStack = new Stack<IUndoableAction>();

		public static void DoAction(object owner, IAction action)
		{
			// InstanceForObject do not return null
			InstanceForObject(owner).DoAction(action);
		}

		private int capacity = 30;

		public void DoAction(IAction action)
		{
			DoAction(action, true);
		}
		public void AddAction(IAction action)
		{
			DoAction(action, false);
		}
		private void DoAction(IAction action, bool perform)
		{
			Do(action, perform, (action is IUndoableAction));
		}
		/*
     * isCanUndo attribute specifies whether the action can be undo.
		 * sometimes an action doesn't need undo even it's a IUndoable action.
		 * the action will not be pushed in undo stack after performed immediately.
		 */
		private void Do(IAction action, bool perform, bool isCanUndo)
		{
			Logger.Log(LOGKEY, string.Format("{0} action: {1}[{2}]", perform ? "do" : "add", action.GetType().Name, action.GetName()));

			if (BeforePerformAction != null)
			{
				var arg = new ActionEventArgs(action, ActionBehavior.Do);
				BeforePerformAction(this, arg);
				if (arg.Cancel) return;
			}

			if (perform) action.Do();

			if (action is IUndoableAction && isCanUndo)
			{
				redoStack.Clear();
				undoStack.Add(action as IUndoableAction);

				if (undoStack.Count() > capacity)
				{
					undoStack.RemoveRange(0, undoStack.Count() - capacity);
					Logger.Log(LOGKEY, "action stack full. remove " + (capacity - undoStack.Count()) + " action(s).");
				}
			}

			if (AfterPerformAction != null) AfterPerformAction(this, new ActionEventArgs(action, ActionBehavior.Do));
		}

		/// <summary>
		/// Clear current action stack
		/// </summary>
		public void Reset()
		{
			redoStack.Clear();
			undoStack.Clear();
		}

		public bool CanUndo()
		{
			return undoStack.Count > 0;
		}

		public bool CanRedo()
		{
			return redoStack.Count() > 0;
		}

		public IAction Undo()
		{
			if (undoStack.Count() > 0)
			{
				IUndoableAction action = null;
				while (undoStack.Count > 0)
				{
					action = undoStack.Last();
					Logger.Log(LOGKEY, "undo action: " + action.ToString());

					if (BeforePerformAction != null)
					{
						var arg = new ActionEventArgs(action, ActionBehavior.Undo);
						BeforePerformAction(this, new ActionEventArgs(action, ActionBehavior.Undo));
						if (arg.Cancel) break;
					}

					undoStack.Remove(action);
					action.Undo();
					redoStack.Push(action);
					if (AfterPerformAction != null) AfterPerformAction(this, new ActionEventArgs(action, ActionBehavior.Undo));

					if (!(action is ISerialUndoAction)) break;
				}
				return action;
			}
			return null;
		}

		public IAction Redo()
		{
			if (redoStack.Count > 0)
			{
				IUndoableAction action = null;
				while (redoStack.Count > 0)
				{
					action = redoStack.Pop();
					Logger.Log(LOGKEY, "redo action: " + action.ToString());

					if (BeforePerformAction != null)
					{
						var arg = new ActionEventArgs(action, ActionBehavior.Redo);
						BeforePerformAction(this, arg);
						if (arg.Cancel) break;
					}

					action.Do();
					undoStack.Add(action);
					if (AfterPerformAction != null) AfterPerformAction(this, new ActionEventArgs(action, ActionBehavior.Redo));

					if (!(action is ISerialUndoAction)) break;
				}
				return action;
			}
			else
				return null;
		}

		public event EventHandler<ActionEventArgs> BeforePerformAction;
		public event EventHandler<ActionEventArgs> AfterPerformAction;
	}

	public class ActionEventArgs : EventArgs
	{
		private IAction action;

		public IAction Action
		{
			get { return action; }
			set { action = value; }
		}

		public ActionBehavior Behavior { get; set; }

		public bool Cancel { get; set; }

		public ActionEventArgs(IAction action, ActionBehavior behavior)
		{
			this.action = action;
			this.Behavior = behavior;
		}
	}

	public enum ActionBehavior
	{
		Do,
		Redo,
		Undo,
	}

	public class ObjectActionEventArgs : ActionEventArgs
	{
		private object Object { get; set; }

		public ObjectActionEventArgs(object obj, IAction action, ActionBehavior behavior)
			: base(action, behavior)
		{
			this.Object = obj;
		}
	}

	public interface IAction
	{
		void Do();
		string GetName();
	}

	public interface IUndoableAction : IAction
	{
		void Undo();
	}

	internal interface ISerialUndoAction : IUndoableAction
	{
	}
	public class ActionGroup : IUndoableAction
	{
		protected List<IAction> actions;

		public List<IAction> Actions
		{
			get { return actions; }
			set { actions = value; }
		}

		private string name;

		public ActionGroup(string name, List<IAction> actions)
		{
			this.name = name;
			this.actions = actions;
		}

		public ActionGroup(string name)
		{
			actions = new List<IAction>();
		}

		public virtual void Do()
		{
			foreach (IAction action in actions)
			{
				action.Do();
			}
		}

		public virtual void Undo()
		{
			for (int i = actions.Count - 1; i >= 0; i--)
				((IUndoableAction)actions[i]).Undo();
		}

		public virtual string GetName()
		{
			return name;
		}

		public override string ToString()
		{
			return string.Format("ActionGroup[" + name + "]");
		}
	}

	public class ActionException : Exception
	{
		private IAction action;

		internal IAction Action
		{
			get { return action; }
			set { action = value; }
		}

		public ActionException(string msg)
			: this(null, msg)
		{
		}

		public ActionException(IAction action, string msg)
			: base(msg)
		{
			this.action = action;
		}
	}
}
