using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;
using TargetedGhostmode;
using UnityEngine;

namespace VirtualBrightPlayz.SCPSL.SCP096Rage
{
    internal class SCP096RageEventHandler : IEventHandlerScp096Panic, IEventHandlerScp096Enrage, IEventHandlerScp096CooldownStart, IEventHandlerUpdate, IEventHandlerPlayerHurt, IEventHandlerPlayerDie
    {
        private SCP096Rage plugin;
        private bool panic = false;
        private List<string> toBeKilled = new List<string>();
        private float timer = 0f;
        private float timer2 = 0f;
        private float timer3 = 0f;

        public SCP096RageEventHandler(SCP096Rage sCP096Rage)
        {
            this.plugin = sCP096Rage;
        }

        void IEventHandlerPlayerDie.OnPlayerDie(PlayerDeathEvent ev)
        {
            if (ev.Player.TeamRole.Role == Role.SCP_096)
            {
                foreach (var plr2 in plugin.Server.GetPlayers())
                {
                    if (Ghostmode.IsHiddenFrom(ev.Player, plr2))
                        Ghostmode.ShowPlayer(ev.Player, plr2);
                }
            }
        }

        void IEventHandlerPlayerHurt.OnPlayerHurt(PlayerHurtEvent ev)
        {
            if (ev.Attacker.TeamRole.Role == Smod2.API.Role.SCP_096 && toBeKilled.Count > 0 && !toBeKilled.Contains(ev.Player.SteamId))
            {
                ev.Damage = 0f;
                ev.DamageType = Smod2.API.DamageType.NONE;
            }
            if (toBeKilled.Contains(ev.Player.SteamId))
            {
                toBeKilled.Remove(ev.Player.SteamId);
            }
        }

        void IEventHandlerScp096CooldownStart.OnScp096CooldownStart(Scp096CooldownStartEvent ev)
        {
            panic = false;
            toBeKilled.Clear();
        }

        void IEventHandlerScp096Enrage.OnScp096Enrage(Scp096EnrageEvent ev)
        {
        }

        void IEventHandlerScp096Panic.OnScp096Panic(Scp096PanicEvent ev)
        {
            if (Scp096PlayerScript.instance != null && Scp096PlayerScript.instance.iAm096)
            {
                GameObject[] plys = PlayerManager.singleton.players;
                int watchingPlayers = 0;
                List<GameObject> watching = new List<GameObject>();
                bool found = false;
                GameObject[] array = plys;
                int i = 0;
                while (i < array.Length)
                {
                    GameObject gameObject = array[i];
                    if (!(gameObject != null) || !gameObject.GetComponent<CharacterClassManager>().IsHuman() || gameObject.GetComponent<FlashEffect>().sync_blind)
                    {
                        goto IL_221;
                    }
                    if (Scp096PlayerScript.instance.sm096Ignored.Count <= 0 || !Scp096PlayerScript.instance.sm096Ignored.Contains(gameObject.GetComponent<CharacterClassManager>().curClass))
                    {
                        Transform transform = gameObject.GetComponent<Scp096PlayerScript>().camera.transform;
                        float num = Scp096PlayerScript.instance.lookingTolerance.Evaluate(Vector3.Distance(transform.position, Scp096PlayerScript.instance.camera.transform.position));
                        RaycastHit raycastHit;
                        if (((double)num >= 0.75 && Vector3.Dot(transform.forward, (transform.position - Scp096PlayerScript.instance.camera.transform.position).normalized) >= -num) || !Physics.Raycast(transform.transform.position, (Scp096PlayerScript.instance.camera.transform.position - transform.position).normalized, out raycastHit, 9999, Scp096PlayerScript.instance.layerMask) || raycastHit.collider.gameObject.layer != 24 || !(raycastHit.collider.GetComponentInParent<Scp096PlayerScript>() == Scp096PlayerScript.instance))
                        {
                            goto IL_221;
                        }
                        found = true;
                        int num2 = watchingPlayers;
                        watchingPlayers = num2 + 1;
                        watching.Add(gameObject);
                        goto IL_221;
                    }
                    IL_23C:
                    i++;
                    continue;
                    IL_221:
                    //yield return 0f;
                    goto IL_23C;
                }
                array = null;
                if (found)
                {
                    panic = true;
                    foreach (var plr in watching)
                    {
                        if (toBeKilled.Contains(plr.GetComponent<CharacterClassManager>().NetworkSteamId)) continue;
                        toBeKilled.Add(plr.GetComponent<CharacterClassManager>().NetworkSteamId);
                    }
                }
                plys = null;
            }
        }

        void IEventHandlerUpdate.OnUpdate(UpdateEvent ev)
        {
            timer -= Time.deltaTime;
            timer2 -= Time.deltaTime;
            timer3 -= Time.deltaTime;
            if (timer2 <= 0f && panic && toBeKilled.Count > 0 && Scp096PlayerScript.instance != null && Scp096PlayerScript.instance.iAm096)
            {
                Scp096PlayerScript.instance.rageProgress = 100f;
                foreach (var plr in plugin.Server.GetPlayers())
                {
                    if (plr.TeamRole.Role == Role.SCP_096)
                    {
                        foreach (var plr2 in plugin.Server.GetPlayers())
                        {
                            if (!Ghostmode.IsHiddenFrom(plr, plr2))
                                Ghostmode.HidePlayer(plr, plr2);
                            if ((plr == plr2 || plr2.TeamRole.Team == Smod2.API.Team.SCP || toBeKilled.Contains(plr2.SteamId)) && Ghostmode.IsHiddenFrom(plr, plr2))
                                Ghostmode.ShowPlayer(plr, plr2);
                        }
                    }
                }
                timer2 = 1f;
            }
            else if (timer <= 0f && toBeKilled.Count == 0 && Scp096PlayerScript.instance != null && Scp096PlayerScript.instance.iAm096)
            {
                Scp096PlayerScript.instance.rageProgress = 0f;
                panic = false;
                Scp096PlayerScript.instance.DeductRage();
                timer2 = 1f;
                foreach (var plr in plugin.Server.GetPlayers())
                {
                    if (plr.TeamRole.Role == Role.SCP_096)
                    {
                        foreach (var plr2 in plugin.Server.GetPlayers())
                        {
                            if (Ghostmode.IsHiddenFrom(plr, plr2))
                                Ghostmode.ShowPlayer(plr, plr2);
                        }
                    }
                }
            }
            if (timer <= 0f)
            {
                timer = toBeKilled.Count + 16f;
                if (Scp096PlayerScript.instance != null && Scp096PlayerScript.instance.iAm096 && panic)
                {
                    foreach (var plr in plugin.Server.GetPlayers())
                    {
                        if (Scp096PlayerScript.instance.GetComponent<CharacterClassManager>().NetworkSteamId.Equals(plr.SteamId))
                        {
                            plr.PersonalBroadcast(1, "Kill these people:", false);
                            foreach (var plyr in toBeKilled)
                            {
                                foreach (var plr2 in plugin.Server.GetPlayers())
                                {
                                    if (plr2.SteamId.Equals(plyr))
                                    {
                                        plr.PersonalBroadcast(1, plr2.Name, false);
                                        plr2.PersonalBroadcast(5, "You are currently \'SCP-096-1\'.", false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (Scp096PlayerScript.instance != null && Scp096PlayerScript.instance.iAm096 && toBeKilled.Count > 0 && panic && timer3 <= 0f)
            {
                timer3 = 2f;
                Scp096PlayerScript.instance.IncreaseRage(1f);
                GameObject[] plys = PlayerManager.singleton.players;
                int watchingPlayers = 0;
                List<GameObject> watching = new List<GameObject>();
                bool found = false;
                GameObject[] array = plys;
                int i = 0;
                while (i < array.Length)
                {
                    GameObject gameObject = array[i];
                    if (!(gameObject != null) || !gameObject.GetComponent<CharacterClassManager>().IsHuman() || gameObject.GetComponent<FlashEffect>().sync_blind)
                    {
                        goto IL_221;
                    }
                    if (Scp096PlayerScript.instance.sm096Ignored.Count <= 0 || !Scp096PlayerScript.instance.sm096Ignored.Contains(gameObject.GetComponent<CharacterClassManager>().curClass))
                    {
                        Transform transform = gameObject.GetComponent<Scp096PlayerScript>().camera.transform;
                        float num = Scp096PlayerScript.instance.lookingTolerance.Evaluate(Vector3.Distance(transform.position, Scp096PlayerScript.instance.camera.transform.position));
                        RaycastHit raycastHit;
                        if (((double)num >= 0.75 && Vector3.Dot(transform.forward, (transform.position - Scp096PlayerScript.instance.camera.transform.position).normalized) >= -num) || !Physics.Raycast(transform.transform.position, (Scp096PlayerScript.instance.camera.transform.position - transform.position).normalized, out raycastHit, 9999, Scp096PlayerScript.instance.layerMask) || raycastHit.collider.gameObject.layer != 24 || !(raycastHit.collider.GetComponentInParent<Scp096PlayerScript>() == Scp096PlayerScript.instance))
                        {
                            goto IL_221;
                        }
                        found = true;
                        int num2 = watchingPlayers;
                        watchingPlayers = num2 + 1;
                        watching.Add(gameObject);
                        goto IL_221;
                    }
                    IL_23C:
                    i++;
                    continue;
                    IL_221:
                    //yield return 0f;
                    goto IL_23C;
                }
                array = null;
                if (found && panic && watching.Count > 1)
                {
                    foreach (var plr in watching)
                    {
                        if (toBeKilled.Contains(plr.GetComponent<CharacterClassManager>().NetworkSteamId)) continue;
                        toBeKilled.Add(plr.GetComponent<CharacterClassManager>().NetworkSteamId);
                        timer = 0f;
                    }
                }
                plys = null;
            }
            else if (Scp096PlayerScript.instance != null && Scp096PlayerScript.instance.iAm096 && panic)
            {
                Scp096PlayerScript.instance.rageProgress = 0f;
                panic = false;
                Scp096PlayerScript.instance.DeductRage();
            }
        }
    }
}