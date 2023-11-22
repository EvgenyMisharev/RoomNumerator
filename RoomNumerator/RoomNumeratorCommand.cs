﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RoomNumerator
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class RoomNumeratorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                GetPluginStartInfo();
            }
            catch { }

            Document doc = commandData.Application.ActiveUIDocument.Document;
            Selection sel = commandData.Application.ActiveUIDocument.Selection;

            List<Room> roomList = new List<Room>();
            roomList = GetRoomsFromCurrentSelection(doc, sel);

            if (roomList.Count == 0)
            {
                RoomSelectionFilter selFilter = new RoomSelectionFilter();
                IList<Reference> selRooms = null;
                try
                {
                    selRooms = sel.PickObjects(ObjectType.Element, selFilter, "Выберите помещения!");
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Cancelled;
                }

                foreach (Reference roomRef in selRooms)
                {
                    roomList.Add(doc.GetElement(roomRef) as Room);
                }
            }

            //Вызов формы
            RoomNumeratorWPF roomNumeratorWPF = new RoomNumeratorWPF();
            roomNumeratorWPF.ShowDialog();
            if (roomNumeratorWPF.DialogResult != true)
            {
                return Result.Cancelled;
            }

            string numberPrefix = roomNumeratorWPF.NumberPrefix;
            string startFrom = roomNumeratorWPF.StartFrom;
            int formatLength = startFrom.Length;
            bool tryChech = int.TryParse(startFrom, out int cnt);
            if (!tryChech) cnt = 1;

            string selectedNumberingDirection = roomNumeratorWPF.SelectedNumberingDirection;
            switch (selectedNumberingDirection)
            {
                case "radioButton_RightAndDown":
                    //Вправо и вниз
                    roomList = roomList.OrderByDescending(r => GetRoomCenter(r)
                        , new CoordinatesBasedComparerXYDown())
                        .ToList();
                    break;
                case "radioButton_DownAndRight":
                    //Вниз и вправо
                    roomList = roomList.OrderByDescending(r => GetRoomCenter(r)
                        , new CoordinatesBasedComparerYXDown())
                        .ToList();
                    break;
                case "radioButton_RightAndUp":
                    //Вправо и вверх
                    roomList = roomList.OrderByDescending(r => GetRoomCenter(r)
                        , new CoordinatesBasedComparerXYUp())
                        .ToList();
                    break;
                case "radioButton_UpAndRight":
                    //Вверх и вправо
                    roomList = roomList.OrderByDescending(r => GetRoomCenter(r)
                        , new CoordinatesBasedComparerYXUp())
                        .ToList();
                    break;
            }

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Нумерация помещений");

                foreach (Room room in roomList)
                {
                    if (numberPrefix == "" || numberPrefix == null)
                    {
                        room.get_Parameter(BuiltInParameter.ROOM_NUMBER).Set($"{cnt}");
                    }
                    else
                    {
                        room.get_Parameter(BuiltInParameter.ROOM_NUMBER).Set($"{numberPrefix}{cnt.ToString($"D{formatLength}")}");
                    }
                    cnt++;
                }

                t.Commit();
            }

            return Result.Succeeded;
        }
        private static List<Room> GetRoomsFromCurrentSelection(Document doc, Selection sel)
        {
            ICollection<ElementId> selectedIds = sel.GetElementIds();
            List<Room> tempRoomsList = new List<Room>();
            foreach (ElementId roomId in selectedIds)
            {
                if (doc.GetElement(roomId) is Room
                    && null != doc.GetElement(roomId).Category
                    && doc.GetElement(roomId).Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Rooms))
                {
                    tempRoomsList.Add(doc.GetElement(roomId) as Room);
                }
            }
            return tempRoomsList;
        }
        private static XYZ GetRoomCenter(Room room)
        {
            XYZ tmpXYZ = null;
            tmpXYZ = (room.get_BoundingBox(null).Max + room.get_BoundingBox(null).Min) / 2;
            return tmpXYZ;
        }
        private static void GetPluginStartInfo()
        {
            // Получаем сборку, в которой выполняется текущий код
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            string assemblyName = "RoomNumerator";
            string assemblyNameRus = "Нумератор помещений";
            string assemblyFolderPath = Path.GetDirectoryName(thisAssembly.Location);

            int lastBackslashIndex = assemblyFolderPath.LastIndexOf("\\");
            string dllPath = assemblyFolderPath.Substring(0, lastBackslashIndex + 1) + "PluginInfoCollector\\PluginInfoCollector.dll";

            Assembly assembly = Assembly.LoadFrom(dllPath);
            Type type = assembly.GetType("PluginInfoCollector.InfoCollector");
            var constructor = type.GetConstructor(new Type[] { typeof(string), typeof(string) });

            if (type != null)
            {
                // Создание экземпляра класса
                object instance = Activator.CreateInstance(type, new object[] { assemblyName, assemblyNameRus });
            }
        }
    }
}
