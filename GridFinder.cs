﻿using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRage.Groups;
using VRageMath;

namespace ALE_GridExporter {

    class GridFinder {

        public static List<MyCubeGrid> FindGridList(string gridName, MyCharacter character, bool includeConnectedGrids) {

            List<MyCubeGrid> grids = new List<MyCubeGrid>();

            if (gridName == null && character == null)
                return new List<MyCubeGrid>();

            if (includeConnectedGrids) {

                ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> groups;

                if (gridName == null)
                    groups = FindLookAtGridGroup(character);
                else
                    groups = FindGridGroup(gridName);

                if (groups.Count > 1)
                    return null;

                foreach (var group in groups) {
                    foreach (var node in group.Nodes) {

                        MyCubeGrid grid = node.NodeData;

                        if (grid.Physics == null)
                            continue;

                        grids.Add(grid);
                    }
                }

            } else {

                ConcurrentBag<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Group> groups;

                if (gridName == null)
                    groups = FindLookAtGridGroupMechanical(character);
                else
                    groups = FindGridGroupMechanical(gridName);

                if (groups.Count > 1)
                    return null;

                foreach (var group in groups) {
                    foreach (var node in group.Nodes) {

                        MyCubeGrid grid = node.NodeData;

                        if (grid.Physics == null)
                            continue;

                        grids.Add(grid);
                    }
                }
            }

            return grids;
        }


    public static ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> FindGridGroup(string gridName) {

            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> groups = new ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group>();
            Parallel.ForEach(MyCubeGridGroups.Static.Physical.Groups, group => {

                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in group.Nodes) {

                    IMyCubeGrid grid = groupNodes.NodeData;

                    if (grid.Physics == null)
                        continue;

                    /* Gridname is wrong ignore */
                    if (!grid.CustomName.Equals(gridName))
                        continue;

                    groups.Add(group);
                }
            });

            return groups;
        }

        public static ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> FindLookAtGridGroup(IMyCharacter controlledEntity) {

            const float range = 5000;
            Matrix worldMatrix;
            Vector3D startPosition;
            Vector3D endPosition;

            worldMatrix = controlledEntity.GetHeadMatrix(true, true, false); // dead center of player cross hairs, or the direction the player is looking with ALT.
            startPosition = worldMatrix.Translation + worldMatrix.Forward * 0.5f;
            endPosition = worldMatrix.Translation + worldMatrix.Forward * (range + 0.5f);

            var list = new Dictionary<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group, double>();
            var ray = new RayD(startPosition, worldMatrix.Forward);

            foreach (var group in MyCubeGridGroups.Static.Physical.Groups) {

                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in group.Nodes) {

                    IMyCubeGrid cubeGrid = groupNodes.NodeData;

                    if (cubeGrid != null) {

                        if (cubeGrid.Physics == null)
                            continue;

                        // check if the ray comes anywhere near the Grid before continuing.    
                        if (ray.Intersects(cubeGrid.WorldAABB).HasValue) {

                            Vector3I? hit = cubeGrid.RayCastBlocks(startPosition, endPosition);

                            if (hit.HasValue) {

                                double distance = (startPosition - cubeGrid.GridIntegerToWorld(hit.Value)).Length();

                                double oldDistance;

                                if (list.TryGetValue(group, out oldDistance)) {

                                    if (distance < oldDistance) {
                                        list.Remove(group);
                                        list.Add(group, distance);
                                    }

                                } else {

                                    list.Add(group, distance);
                                }
                            }
                        }
                    }
                }
            }

            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> bag = new ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group>();

            if (list.Count == 0)
                return bag;

            // find the closest Entity.
            var item = list.OrderBy(f => f.Value).First();
            bag.Add(item.Key);

            return bag;
        }

        public static ConcurrentBag<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Group> FindGridGroupMechanical(string gridName) {

            ConcurrentBag<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Group> groups = new ConcurrentBag<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Group>();
            Parallel.ForEach(MyCubeGridGroups.Static.Mechanical.Groups, group => {

                foreach (MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node groupNodes in group.Nodes) {

                    IMyCubeGrid grid = groupNodes.NodeData;

                    if (grid.Physics == null)
                        continue;

                    /* Gridname is wrong ignore */
                    if (!grid.CustomName.Equals(gridName))
                        continue;

                    groups.Add(group);
                }
            });

            return groups;
        }

        public static ConcurrentBag<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Group> FindLookAtGridGroupMechanical(IMyCharacter controlledEntity) {

            const float range = 5000;
            Matrix worldMatrix;
            Vector3D startPosition;
            Vector3D endPosition;

            worldMatrix = controlledEntity.GetHeadMatrix(true, true, false); // dead center of player cross hairs, or the direction the player is looking with ALT.
            startPosition = worldMatrix.Translation + worldMatrix.Forward * 0.5f;
            endPosition = worldMatrix.Translation + worldMatrix.Forward * (range + 0.5f);

            var list = new Dictionary<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Group, double>();
            var ray = new RayD(startPosition, worldMatrix.Forward);

            foreach (var group in MyCubeGridGroups.Static.Mechanical.Groups) {

                foreach (MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node groupNodes in group.Nodes) {

                    IMyCubeGrid cubeGrid = groupNodes.NodeData;

                    if (cubeGrid != null) {

                        if (cubeGrid.Physics == null)
                            continue;

                        // check if the ray comes anywhere near the Grid before continuing.    
                        if (ray.Intersects(cubeGrid.WorldAABB).HasValue) {

                            Vector3I? hit = cubeGrid.RayCastBlocks(startPosition, endPosition);

                            if (hit.HasValue) {

                                double distance = (startPosition - cubeGrid.GridIntegerToWorld(hit.Value)).Length();

                                double oldDistance;

                                if (list.TryGetValue(group, out oldDistance)) {

                                    if (distance < oldDistance) {
                                        list.Remove(group);
                                        list.Add(group, distance);
                                    }

                                } else {

                                    list.Add(group, distance);
                                }
                            }
                        }
                    }
                }
            }

            ConcurrentBag<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Group> bag = new ConcurrentBag<MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Group>();

            if (list.Count == 0)
                return bag;

            // find the closest Entity.
            var item = list.OrderBy(f => f.Value).First();
            bag.Add(item.Key);

            return bag;
        }
    }
}
