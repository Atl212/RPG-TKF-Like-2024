﻿using System.IO;
using UnityEngine;

namespace Gyvr.Mythril2D
{
    public class SaveSystem : AGameSystem
    {
        public string saveFileName => m_saveFileName;
        private string m_saveFileName = "SAVEFILE_A";

        public void SetSaveFileName()
        {
        }

        public void LoadDefaultSaveFile(SaveFile saveFile, string saveFileName)
        {
            SaveFileData newSaveFile = DuplicateSaveFile(saveFile.content);

            LoadSaveFile(newSaveFile, saveFileName);
        }

        /**
         * Never use the m_defaultSaveFile as-is, but instead, always duplicate it (deep copy) to prevent changing the initial scriptable object data.
         * This is especially useful in editor. (TODO: make it #if UNITY_EDITOR, otherwise directly use the data without cloning it)
         */
        public SaveFileData DuplicateSaveFile(SaveFileData saveFile)
        {
            string saveData = JsonUtility.ToJson(saveFile, true);
            return JsonUtility.FromJson<SaveFileData>(saveData);
        }

        public static void EraseSaveData(string saveFileName)
        {
            string filepath = Path.GetFullPath(Path.Combine(Application.persistentDataPath, saveFileName));

            File.Delete(filepath);
        }

        public static bool TryExtractingSaveData(string saveFileName, out SaveFileData output)
        {
            string filepath = Path.GetFullPath(Path.Combine(Application.persistentDataPath, saveFileName));

            if (!File.Exists(filepath))
            {
                output = new SaveFileData { };
                return false;
            }

            try
            {
                using (FileStream stream = new FileStream(filepath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string dataToLoad = reader.ReadToEnd();
                        output = JsonUtility.FromJson<SaveFileData>(dataToLoad);
                        return true;
                    }
                }
            }
            catch
            {
                output = new SaveFileData { };
                return false;
            }
        }

        public void LoadFromFile(string saveFileName)
        {

            Debug.Log(saveFileName);

            m_saveFileName = saveFileName;

            Debug.Log($"Loading from {saveFileName}...");

            SaveFileData saveFile;

            if (TryExtractingSaveData(saveFileName, out saveFile))
            {
                LoadSaveFile(saveFile);
                Debug.Log($"Loading succeeded!");
            }
            else
            {
                Debug.LogError($"Loading failed!");
            }
        }

        public void SaveToFile(string saveFileName)
        {
            GameManager.NotificationSystem.saveStart.Invoke();

            m_saveFileName = saveFileName;
            
            // 测试的时候没有绑定存档所以为空
            //Debug.Log("saveFileName = " + saveFileName);

            string filepath = Path.GetFullPath(Path.Combine(Application.persistentDataPath, saveFileName));

            Debug.Log($"Saving to {filepath}...");

            try
            {
                SaveFileData saveFile = CreateSaveFile();

                string dataToStore = JsonUtility.ToJson(saveFile, true);

                using (FileStream stream = new FileStream(filepath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(dataToStore);

                        Debug.Log($"Saving succeeded!");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Saving failed: {e.Message}");
            }

            GameManager.NotificationSystem.saveEnd.Invoke();
        }

        public void LoadSaveFile(SaveFileData saveFile)
        {
            GameManager.GameFlagSystem.LoadDataBlock(saveFile.gameFlags);
            GameManager.WarehouseSystem.LoadDataBlock(saveFile.warehouse);
            GameManager.InventorySystem.LoadDataBlock(saveFile.inventory);
            GameManager.JournalSystem.LoadDataBlock(saveFile.journal);
            GameManager.PlayerSystem.LoadDataBlock(saveFile.player);
            GameManager.TeleportLoadingSystem.RequestTransition(saveFile.map, null, null, null, ETeleportType.Revival);
            //GameManager.TeleportLoadingSystem.RequestTransition(saveFile.map);
        }

        public void LoadSaveFile(SaveFileData saveFile, string saveFileName)
        {
            Debug.Log(saveFileName);

            m_saveFileName = saveFileName;

            GameManager.GameFlagSystem.LoadDataBlock(saveFile.gameFlags);
            GameManager.WarehouseSystem.LoadDataBlock(saveFile.warehouse);
            GameManager.InventorySystem.LoadDataBlock(saveFile.inventory);
            GameManager.JournalSystem.LoadDataBlock(saveFile.journal);
            GameManager.PlayerSystem.LoadDataBlock(saveFile.player);
            GameManager.TeleportLoadingSystem.RequestTransition(saveFile.map, null, null, () =>
            {
                GameManager.SaveSystem.SaveToFile(saveFileName);
            }, ETeleportType.Revival);
            //GameManager.TeleportLoadingSystem.RequestTransition(saveFile.map);
        }

        private string GenerateSavefileHeader()
        {
            string header;

            Hero player = GameManager.PlayerSystem.PlayerInstance;

            header = string.Format("{0} {1}{2}",
                player.characterSheet.displayName,
                GameManager.Config.GetTermDefinition("level").shortName,
                player.level
                );

            return header;
        }

        private SaveFileData CreateSaveFile()
        {
            return new SaveFileData
            {
                header = GenerateSavefileHeader(),
                map = GameManager.TeleportLoadingSystem.GetCurrentMapName(),
                gameFlags = GameManager.GameFlagSystem.CreateDataBlock(),
                warehouse = GameManager.WarehouseSystem.CreateDataBlock(),
                inventory = GameManager.InventorySystem.CreateDataBlock(),
                journal = GameManager.JournalSystem.CreateDataBlock(),
                player = GameManager.PlayerSystem.CreateDataBlock()
            };
        }
    }
}
