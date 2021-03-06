using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using Equipment = StagesMod.Equipments.Equipment;
using Item = StagesMod.Items.Item;

namespace StagesMod
{
    public class InitPickups
    {
        public static Dictionary<ItemDef, Item> itemList = new Dictionary<ItemDef, Item>();
        public static Dictionary<EquipmentDef, Equipment> equipmentList = new Dictionary<EquipmentDef, Equipment>();
        public static SerializableContentPack contentPack { get; set; } = StagesMod.serializableContentPack;

        public static void Init()
        {
            InitializeEquipments();
            InitializeItems();
        }

        [SystemInitializer(typeof(PickupCatalog))]
        private static void HookInit()
        {
            SMLog.LogI("Subscribing to delegates related to Items and Equipments.");
            CharacterBody.onBodyStartGlobal += AddItemManager;
            On.RoR2.CharacterBody.RecalculateStats += OnRecalculateStats;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += FireTurboEqp;
        }

        public static IEnumerable<Item> InitializeItems()
        {
            SMLog.LogD($"Getting the Items found inside {Assembly.GetExecutingAssembly()}...");
            Assembly.GetExecutingAssembly().GetTypes()
                           .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Item)))
                           .Where(type => !type.GetCustomAttributes(true)
                                    .Select(obj => obj.GetType())
                                    .Contains(typeof(DisabledContent)))
                           .Select(itemType => (Item)Activator.CreateInstance(itemType)).ToList().ForEach(item => AddItem(item, contentPack));
            return null;
        }

        public static void AddItem(Item item, SerializableContentPack contentPack, Dictionary<ItemDef, Item> itemDictionary = null)
        {
            item.Initialize();
            //HG.ArrayUtils.ArrayAppend(ref TurboEdition.serializableContentPack.itemDefs, item.itemDef);
            itemList.Add(item.itemDef, item);
            if (itemDictionary != null)
                itemDictionary.Add(item.itemDef, item);
            SMLog.LogD($"Item {item.itemDef} added to {contentPack.name}");
        }

        public static IEnumerable<Equipment> InitializeEquipments()
        {
            SMLog.LogD($"Getting the Equipments found inside {Assembly.GetExecutingAssembly()}...");
            Assembly.GetExecutingAssembly().GetType().Assembly.GetTypes()
                           .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Equipment)))
                           .Where(type => !type.GetCustomAttributes(true)
                                    .Select(obj => obj.GetType())
                                    .Contains(typeof(DisabledContent)))
                           .Select(eqpType => (Equipment)Activator.CreateInstance(eqpType)).ToList().ForEach(eqp => AddEquipment(eqp, contentPack));
            return null;
        }

        public static void AddEquipment(Equipment equip, SerializableContentPack contentPack, Dictionary<EquipmentDef, Equipment> equipDictionary = null)
        {
            equip.Initialize();
            //HG.ArrayUtils.ArrayAppend(ref contentPack.equipmentDefs, equip.equipmentDef);
            equipmentList.Add(equip.equipmentDef, equip);
            if (equipDictionary != null)
                equipDictionary.Add(equip.equipmentDef, equip);
            SMLog.LogD($"Equipment {equip.equipmentDef} added to {contentPack.name}");
        }

        private static void AddItemManager(CharacterBody body)
        {
            if (!body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Masterless) && body.master.inventory)
            {
                var itemManager = body.gameObject.AddComponent<SMItemManager>();
                itemManager.CheckForTEItems();
            }
        }

        private static void OnRecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            var manager = self.GetComponent<SMItemManager>();
            var buffManager = self.GetComponent<SMBuffManager>();
            manager?.RunStatRecalculationsStart();
            buffManager?.RunStatRecalculationsStart(self);
            orig(self);
            manager?.RunStatRecalculationsEnd();
            buffManager?.RunStatRecalculationsEnd();
        }

        private static bool FireTurboEqp(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Boolean RoR2.EquipmentSlot::PerformEquipmentAction(RoR2.EquipmentDef)' called on client");
                return false;
            }
            Equipment equipment;
            if (equipmentList.TryGetValue(equipmentDef, out equipment))
            {
                var body = self.characterBody;
                return equipment.FireAction(self);
            }
            return orig(self, equipmentDef);
        }
    }
}