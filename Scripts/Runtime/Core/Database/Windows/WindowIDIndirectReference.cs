using System;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    [Serializable]
    public sealed class WindowIDIndirectReference : CollectionItemIndirectReference<WindowID>
    {
        public WindowIDIndirectReference(WindowID item) : base(item)
        {
        }
    }
}
