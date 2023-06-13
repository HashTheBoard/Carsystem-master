using BrokeProtocol.API;
using BrokeProtocol.Entities;
using BrokeProtocol.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarSystem.Fuel
{

    public class Fuel : PlayerEvents
    {
        Main main;
        List<string> refilling = new List<string>();

        public Fuel() => main = Main.Instance;

        PlayerEvents d = new PlayerEvents();

        [Execution(ExecutionMode.Event)]
        public override bool Mount(ShPlayer player, ShMountable mount, byte seat)
        {
            if (mount.isHuman || main.config.blacklistedcars.Contains(mount.name)) return false;
            int maxlvl;
            if (main.config.fueltank != null && main.config.fueltank.ContainsKey(mount.name)) main.config.fueltank.TryGetValue(mount.name, out maxlvl);
            else maxlvl = 100;

            player.svPlayer.StartCoroutine(FuelCoroutine(player, mount, maxlvl));
            return true;
        }

        [Execution(ExecutionMode.Additive)]
        public override bool Dismount(ShPlayer player)
        {
            player.svPlayer.DestroyTextPanel("mmc");
            return true;
        }
        private IEnumerator FuelCoroutine(ShPlayer player, ShMountable mount, int maxlvl)
        {
            while (player.IsDriving)
            {
                if (mount is ShTransport transport)
                {
                    if (!transport.svEntity.CustomData.TryFetchCustomData<float>("fuelevel", out float fuel, null))
                    {
                        transport.svEntity.CustomData.AddOrUpdate("fuelevel", maxlvl);
                        DisplayUi(transport, player, fuel, maxlvl);
                    }
                    else
                    {
                        if (fuel <= 0)
                        {
                            player.svPlayer.StartCoroutine(StopCar(player, transport));
                            if (fuel >= -5 && fuel <= 0) player.svPlayer.SendGameMessage(main.config.EmptyFuel);
                        }

                        DisplayUi(transport, player, fuel, maxlvl);
                        transport.svEntity.CustomData.AddOrUpdate<float>("fuelevel", fuel - 0.1f);
                    }
                    yield return new WaitForSeconds(0.78f);
                }
            }
            player.svPlayer.DestroyTextPanel();
            yield break;
        }

        private IEnumerator StopCar(ShPlayer player, ShTransport transport)
        {
            while (player.IsDriving)
            {
                transport.svMountable.SvRelocate(transport.svMountable.transform, null);
                yield return new WaitForSeconds(0.3f);
            }
        }

        [CustomTarget]
        public void StationEnter(ShEntity trigger, ShPhysical physical)
        {
            if (physical is ShPlayer p)
            {
                if (p.curMount is ShTransport trans)
                {
                    if (trans.svEntity.CustomData.TryFetchCustomData<float>("fuelevel", out float fuel, null))
                    {
                        refilling.Add(p.username);
                        if (p.curMount.isHuman || main.config.blacklistedcars.Contains(p.curMount.name)) return;
                        int maxlvl;
                        if (main.config.fueltank != null && main.config.fueltank.ContainsKey(p.curMount.name)) main.config.fueltank.TryGetValue(p.curMount.name, out maxlvl);
                        else maxlvl = 100;

                        p.svPlayer.StartCoroutine(FuelRefilCoroutine(trans, p, maxlvl));
                    }
                }
            }
        }

        [CustomTarget]
        public void StationExit(ShEntity trigger, ShPhysical physical)
        {
            if (physical is ShPlayer p)
            {
                if (refilling.Contains(p.username)) refilling.Remove(p.username);
            }
        }

        private IEnumerator FuelRefilCoroutine(ShTransport trans, ShPlayer p, int maxlvl)
        {
            while (refilling.Contains(p.username) && p.IsDriving)
            {
                float fuel;
                if (!trans.svEntity.CustomData.TryFetchCustomData<float>("fuelevel", out fuel, null))
                    if (fuel >= maxlvl) { p.svPlayer.SendGameMessage("&4Résérvoir Plein !"); yield return new WaitForSeconds(2.5f); }

                if (p.MyMoneyCount > 10)
                {
                    p.TransferMoney(DeltaInv.RemoveFromMe, 10);
                    trans.svEntity.CustomData.AddOrUpdate("fuelevel", fuel + 1.0f);
                    p.svPlayer.SendGameMessage("+1 litre Fuel");
                    yield return new WaitForSeconds(2.0f);
                }
                else { p.svPlayer.SendGameMessage("&4Fond insuffisent !"); yield return new WaitForSeconds(2.5f); }
            }

            yield break;
        }

        void DisplayUi(ShTransport transport, ShPlayer player, float fuel, int maxlvl)
        {
            float ScaleSpeed = transport.Velocity.magnitude;
            string speed = String.Format("{00}", Math.Round(ScaleSpeed * 2.85f, 0f));
            string sfuel = String.Format("{00}", Math.Round(fuel, 0f));
            if (ScaleSpeed < 70) player.svPlayer.SendTextPanel($"&3{transport.name} \n\n &2{speed} Km/h \n\n &8Essence: {sfuel}/{maxlvl}", "mmc");
            else if (ScaleSpeed < 110) player.svPlayer.SendTextPanel($"&3{transport.name} \n\n &e{speed} Km/h \n\n &8Essence: {sfuel}/{maxlvl}", "mmc");
            else player.svPlayer.SendTextPanel($"&3{transport.name} \n\n &c{speed} Km/h \n\n &8Essence: {sfuel}/{maxlvl}", "mmc");
        }
    }
}
