﻿using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod;
using Torch.Mod.Messages;
using VRage.Game.ModAPI;

namespace ALE_GridExporter {

    public class GridExporterCommands : CommandModule {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public GridExporterPlugin Plugin => (GridExporterPlugin) Context.Plugin;

        [Command("exportgrid", "Exports the gridgroup with given name or you are looking at to a file with the given name.")]
        [Permission(MyPromoteLevel.SpaceMaster)]
        public void ExportGrid(string filename, string gridName = null) {

            MyCharacter character = null;

            if (gridName == null) {

                if (Context.Player == null) {
                    Context.Respond("You need to enter a Grid name where the grid will be spawned at.");
                    return;
                }

                var player = ((MyPlayer)Context.Player).Identity;

                if (player.Character == null) {
                    Context.Respond("Player has no character to spawn the grid close to!");
                    return;
                }

                character = player.Character;
            }

            List<MyCubeGrid> grids = GridFinder.FindGridList(gridName, character, Plugin.Config.IncludeConnectedGrids);

            if(grids == null) {
                Context.Respond("Multiple grids found. Try to rename them first or try a different subgrid for identification!");
                return;
            }

            if(grids.Count == 0) {
                Context.Respond("No grids found. Check your viewing angle or try the correct name!");
                return;
            }

            if (GridManager.SaveGrid(Plugin.CreatePath(filename), filename, Plugin.Config.KeepOriginalOwner, grids, Context))
                Context.Respond("Export Complete!");
            else
                Context.Respond("Export Failed!");
        }

        [Command("importgrid", "Imports a the grid with given name or you are looking at to a file with the given name.")]
        [Permission(MyPromoteLevel.SpaceMaster)]
        public void ImportGrid(string filename, string playerName = null) {

            MyIdentity player;

            if (playerName == null) {

                if (Context.Player == null) {
                    Context.Respond("You need to enter a Player name where the grid will be spawned at.");
                    return;
                }

                player = ((MyPlayer) Context.Player).Identity;

            } else {

                player = PlayerUtils.GetIdentityByName(playerName);

                if(player == null) {
                    Context.Respond("Player not Found!");
                    return;
                }
            }

            if (player.Character == null) {
                Context.Respond("Player has no character to spawn the grid close to!");
                return;
            }

            var playerPosition = player.Character.PositionComp.GetPosition();

            if(GridManager.LoadGrid(Plugin.CreatePath(filename), playerPosition, Context))
                Context.Respond("Import Complete!");
            else
                Context.Respond("Import Failed!");
        }
    }
}
