using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Cascade.Editor.Authoring
{
    // Notifications 2.x leaves SettingsProvider Undo delegates attached after destroying its manager.
    // Remove only callbacks whose exact package target is already destroyed; active settings stay intact.
    [InitializeOnLoad]
    public static class NotificationUndoGuard
    {
        static double next;
        static NotificationUndoGuard(){EditorApplication.update+=Tick;}
        static void Tick(){if(EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+2;Prune();}
        public static void Prune()
        {
            var callbacks=Undo.undoRedoPerformed?.GetInvocationList();if(callbacks==null)return;
            foreach(var callback in callbacks) {
                var target=callback.Target;var type=target?.GetType();
                if(type?.FullName!="Unity.Notifications.NotificationSettingsProvider")continue;
                var field=type.GetField("m_SettingsManager",BindingFlags.NonPublic|BindingFlags.Instance);
                if(field!=null&&(field.GetValue(target) as Object)==null)Undo.undoRedoPerformed-=(Undo.UndoRedoCallback)callback;
            }
        }
    }
}
