#if   Il2Cpp
using Il2CppFishNet;
using Il2CppFishNet.Transporting;
using Il2CppScheduleOne.Employees;
using Il2CppScheduleOne.Management;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.ObjectScripts;
using Behaviour = Il2CppScheduleOne.NPCs.Behaviour.Behaviour;
using Il2CppInterop.Runtime.InteropTypes;

#elif Mono
using FishNet;
using FishNet.Transporting;
using ScheduleOne.Employees;
using ScheduleOne.Management;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using Behaviour = ScheduleOne.NPCs.Behaviour.Behaviour;

#endif
using System;
using System.Collections;
using UnityEngine;
using MelonLoader;

namespace PackagersLoadVehicles
{
    public class UnpackagingStationBehaviour : Behaviour
    {
        private Coroutine routine;
        public PackagingStation Station { get; private set; }
        public bool InProgress { get; private set; }

        public override void Begin()  { base.Begin();  StartOp(); }
        public override void Resume() { base.Resume(); StartOp(); }
        public override void Pause()  { base.Pause(); if (InProgress) StopOp(); }

        public override void End()
        {
            base.End();
            if (InProgress) StopOp();
            if (InstanceFinder.IsServer && Station != null && Station.NPCUserObject == Npc.NetworkObject)
                Station.SetNPCUser(null);
        }

        public void AssignStation(PackagingStation station) => Station = station;

        public override void ActiveMinPass()
        {
            base.ActiveMinPass();
            if (!InstanceFinder.IsServer || InProgress) return;

            if (IsStationReadyForUnpack(Station))
            {
                if (!Npc.Movement.IsMoving)
                {
                    if (IsAtStation()) BeginUnpack();
                    else SetDestination(Station.StandPoint.position);
                }
            }
            else Disable_Networked(null);
        }

        private void StartOp()
        {
            if (!InstanceFinder.IsServer) return;

            if (!IsStationReadyForUnpack(Station))
            {
                MelonLogger.Warning(Npc.fullName + " has no station to unpackage at");
                Disable_Networked(null);
            }
            else Station.SetNPCUser(Npc.NetworkObject);
        }

        private bool IsAtStation() => Npc.Movement.IsAsCloseAsPossible(Station.StandPoint.position);

        private bool IsStationReadyForUnpack(PackagingStation s)
        {
            if (s == null) return false;

            if (s.GetState(PackagingStation.EMode.Unpackage) != PackagingStation.EState.CanBegin)
                return false;

            var usable = (s as Il2CppObjectBase)?.TryCast<IUsable>();
            if (usable != null && usable.IsInUse && s.NPCUserObject != Npc.NetworkObject)
                return false;

            return Npc.Movement.CanGetTo(s.StandPoint.position);
        }

        public void BeginUnpack()
        {
            if (InProgress || Station == null) return;
            InProgress = true;
            Npc.Movement.FaceDirection(Station.StandPoint.forward);
            routine = StartCoroutine((Il2CppSystem.Collections.IEnumerator)DoUnpack());
        }

        private IEnumerator DoUnpack()
        {
            yield return new WaitForEndOfFrame();
            Npc.Avatar.Anim.SetBool("UsePackagingStation", true);

            float t = PackagingStationBehaviour.BASE_PACKAGING_TIME /
                      ((Npc as Packager).PackagingSpeedMultiplier * Station.PackagerEmployeeSpeedMultiplier);
            for (float i = 0f; i < t; i += Time.deltaTime)
            {
                Npc.Avatar.LookController.OverrideLookTarget(Station.Container.position, 0);
                yield return new WaitForEndOfFrame();
            }

            Npc.Avatar.Anim.SetBool("UsePackagingStation", false);
            if (InstanceFinder.IsServer)
                Station.Unpack();

            InProgress = false;
            routine = null;
        }

        private void StopOp()
        {
            if (routine != null) StopCoroutine(routine);
            Npc.Avatar.Anim.SetBool("UsePackagingStation", false);
            if (InstanceFinder.IsServer && Station != null && Station.NPCUserObject == Npc.NetworkObject)
                Station.SetNPCUser(null);
            InProgress = false;
        }
    }
}