using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UIConfigurator
{
    public class JSONConfigManager
    {
        //Path for saving the json.
        private string configFilePath;
        //Setting used when saving the JSON.
        private JsonSerializerSettings jsonSettings;
        //Settings for all UI rects, mirrored with the JSON file.
        private Dictionary<string, SimplifiedRectTransformSettings> rectTransformSettings = new Dictionary<string, SimplifiedRectTransformSettings>(StringComparer.OrdinalIgnoreCase);

        public JSONConfigManager(string configFilePath)
        {
            this.configFilePath = configFilePath;

            jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            LoadSettings();
        }
        public void LoadSettings()
        {
            if (File.Exists(configFilePath))
            {
                string json = File.ReadAllText(configFilePath);
                rectTransformSettings = JsonConvert.DeserializeObject<Dictionary<string, SimplifiedRectTransformSettings>>(json, jsonSettings);
            }
        }

        public void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(rectTransformSettings, Formatting.Indented, jsonSettings);
            File.WriteAllText(configFilePath, json);
        }

        public void SetAnchorMin(RectTransform rectTransform)
        {
            rectTransformSettings[rectTransform.name.ToLower()].currentAnchorMin = new SimplifiedVector2(rectTransform.anchorMin);
        }

        public void SetAnchorMax(RectTransform rectTransform)
        {
            rectTransformSettings[rectTransform.name.ToLower()].currentAnchorMax = new SimplifiedVector2(rectTransform.anchorMax);
        }

        public void SetAnchors(RectTransform rectTransform)
        {
            SetAnchorMin(rectTransform);
            SetAnchorMax(rectTransform);
        }

        public void SaveOriginalSettings(RectTransform rect)
        {
            string rectName = rect.name.ToLower();
            if (!rectTransformSettings.ContainsKey(rectName))
            {
                SimplifiedRectTransformSettings settings = new SimplifiedRectTransformSettings
                {
                    originalAnchorMin = new SimplifiedVector2(rect.anchorMin),
                    originalAnchorMax = new SimplifiedVector2(rect.anchorMax),
                    currentAnchorMin = new SimplifiedVector2(rect.anchorMin),
                    currentAnchorMax = new SimplifiedVector2(rect.anchorMax)
                };
                rectTransformSettings.Add(rectName, settings);
            }
        }

        public void ResetAllSettings()
        {
            foreach(KeyValuePair<string, SimplifiedRectTransformSettings> setting in rectTransformSettings)
            {
                rectTransformSettings[setting.Key].currentAnchorMin = new SimplifiedVector2(rectTransformSettings[setting.Key].originalAnchorMin.ToVector2());
                rectTransformSettings[setting.Key].currentAnchorMax = new SimplifiedVector2(rectTransformSettings[setting.Key].originalAnchorMax.ToVector2());
            }
        }

        public void ResetToOriginal(RectTransform rect)
        {
            string rectName = rect.name.ToLower();
            rect.anchorMin = rectTransformSettings[rectName].originalAnchorMin.ToVector2();
            rect.anchorMax = rectTransformSettings[rectName].originalAnchorMax.ToVector2();
            rectTransformSettings[rectName].currentAnchorMin = new SimplifiedVector2(rect.anchorMin);
            rectTransformSettings[rectName].currentAnchorMax = new SimplifiedVector2(rect.anchorMax);
        }

        public void ApplySettings(RectTransform rect)
        {
            string rectName = rect.name.ToLower();
            var settings = rectTransformSettings[rectName];
            rect.anchorMin = settings.currentAnchorMin.ToVector2();
            rect.anchorMax = settings.currentAnchorMax.ToVector2();
        }

        public void RectAdded(RectTransform rect)
        {
            string rectName = rect.name.ToLower();
            if (rectTransformSettings.ContainsKey(rectName))
            {
                ApplySettings(rect);
            }
            else
            {
                SaveOriginalSettings(rect);
            }
        }
    }

    [System.Serializable]
    public class SimplifiedVector2
    {
        public float x;
        public float y;

        public SimplifiedVector2() { }

        public SimplifiedVector2(Vector2 vector)
        {
            x = vector.x;
            y = vector.y;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
    }

    [System.Serializable]
    public class SimplifiedRectTransformSettings
    {
        public SimplifiedVector2 originalAnchorMin;
        public SimplifiedVector2 originalAnchorMax;
        public SimplifiedVector2 currentAnchorMin;
        public SimplifiedVector2 currentAnchorMax;
    }
}
