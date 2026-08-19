using System;
using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    [Serializable]
    public class VFX_Entry
    {
        public VFX_Type type;
        public Animator animator;
        public string triggerName = "Play";
    }

    [SerializeField] private VFX_Entry[] entries;


    public void Play(VFX_Type type)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].type != type) continue;

            var a = entries[i].animator;
            if (a == null) return;

            a.ResetTrigger(entries[i].triggerName);
            a.SetTrigger(entries[i].triggerName);
            return;
        }

        Debug.LogWarning($"VFX_Entry not found : {type}", this);
    }
}
