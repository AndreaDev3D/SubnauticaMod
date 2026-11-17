using HarmonyLib;
using UnityEngine;

namespace AD3D_TransportSolution.Runtime
{
    // Reads an object's saved id safely. The game's UniqueIdentifier.Id getter makes up a NEW id when the id is still
    // empty, and during a load our components already run before the saved id is applied: an object that asked then
    // kept a made-up id, so whatever the mod had saved against its real id (a pylon's rails) was lost on every load.
    internal static class SaveIds
    {
        private static readonly AccessTools.FieldRef<UniqueIdentifier, string> RawId =
            AccessTools.FieldRefAccess<UniqueIdentifier, string>("id");

        // The saved id; null while the world is still loading or the id isn't applied yet. allowNew: the object was
        // made in this session, so it may be given a fresh id now.
        public static string Of(Component c, bool allowNew = false)
        {
            var identifier = c != null ? c.GetComponent<PrefabIdentifier>() : null;
            if (identifier == null || WaitScreen.IsWaiting)
                return null;
            var id = RawId(identifier);
            if (!string.IsNullOrEmpty(id))
                return id;
            return allowNew ? identifier.Id : null;
        }
    }
}
