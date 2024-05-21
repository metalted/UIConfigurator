using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace UIConfigurator
{
    public class UIConfigurator : MonoBehaviour
    {
        //The color of the border thats placed around the UI when selected.
        public Color borderColor = Color.red;
        private Color animatedBorderColor;
        private float animatedBorderFadeDuration = 1.0f;

        //The collection of editable rect transforms.
        private List<RectTransform> rectTransforms = new List<RectTransform>();
        //The rect we are currently editing.
        private RectTransform currentRect;
        //The index used for cycling.
        private int currentRectIndex = -1;

        //The handles we use to edit.
        private GameObject moveHandle;
        private GameObject scaleHandle;
        private GameObject resetHandle;

        //Current state of editing.
        private bool isEditing = false;
        private bool isRectEditing = false;
        private bool isMoving = false;
        private bool isScaling = false;

        //Used for calculating size and position changes.
        private Vector2 initialMousePosition;
        private Vector2 initialAnchorMin;
        private Vector2 initialAnchorMax;
       
        //JSON Manager
        public JSONConfigManager configManager;

        //Original cursor settings
        private bool preEditCursorVisibility = false;
        private CursorLockMode preEditCursorLockMode = CursorLockMode.None;
        private bool preEditRectActive = false;
        private bool setEditPlaceHolderText = false;
        private string preEditTextContent = "";

        //Singleton and Setup
        private static UIConfigurator instance;
        public static UIConfigurator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<UIConfigurator>();
                }
                return instance;
            }
        }       

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            //Create a new config manager.
            configManager = new JSONConfigManager(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"BepInEx\plugins", "UIConfiguratorSettings.json"));
            animatedBorderColor = borderColor;
        }

        public void SceneChange()
        {
            if(isEditing)
            {
                ToggleEditMode();
            }
        }

        void Update()
        {
            if (Input.GetKeyDown((KeyCode) Plugin.Instance.editModeKey.BoxedValue))
            {
                ToggleEditMode();
            }

            if (isEditing)
            {
                if (isRectEditing)
                {
                    if (Input.GetKeyDown((KeyCode)Plugin.Instance.nextCycleKey.BoxedValue))
                    {
                        CycleToNextRect();
                    }
                    else if (Input.GetKeyDown((KeyCode)Plugin.Instance.prevCycleKey.BoxedValue))
                    {
                        CycleToPreviousRect();
                    }

                    HandleEditing();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    StopEditing();
                }

                //Animate the border color.
                float alpha = Mathf.PingPong(Time.time / animatedBorderFadeDuration, 1.0f);
                animatedBorderColor = new Color(animatedBorderColor.r, animatedBorderColor.g, animatedBorderColor.b, alpha);
            }
        }

        public void SetBorderColor(Color color)
        {
            borderColor = color;
            animatedBorderColor = color;
        }

        private void ToggleEditMode()
        {
            if (!isEditing)
            {
                isEditing = true;
                preEditCursorVisibility = Cursor.visible;
                preEditCursorLockMode = Cursor.lockState;

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                if (rectTransforms.Count > 0)
                {
                    //Check if any of the rects are active in the hierarchy
                    int firstActiveIndex = -1;
                    for(int i = 0; i < rectTransforms.Count; i++)
                    {
                        if(rectTransforms[i] != null)
                        {
                            if (rectTransforms[i].parent.gameObject.activeInHierarchy)
                            {
                                firstActiveIndex = i;
                                break;
                            }
                        }
                    }

                    if(firstActiveIndex != -1)
                    {
                        currentRectIndex = firstActiveIndex;
                        EnterRectEditMode(rectTransforms[currentRectIndex]);
                    }
                    else
                    {
                        PlayerManager.Instance.messenger.Log("No configurable UI available.", 2f);
                    }
                }
                else
                {
                    PlayerManager.Instance.messenger.Log("No configurable UI available.", 2f);
                }
            }
            else
            {
                isEditing = false;

                Cursor.visible = preEditCursorVisibility;
                Cursor.lockState = preEditCursorLockMode;

                PlayerManager.Instance.messenger.Log("Saving UIConfig!", 2f);

                configManager.SaveSettings();

                DeactivateHandles();

                if(currentRect != null)
                {
                    currentRect.gameObject.SetActive(preEditRectActive);
                }                

                isRectEditing = false;
                currentRect = null;
                currentRectIndex = -1;
            }
        }

        private void CycleToNextRect()
        {
            if (rectTransforms.Count == 0) return;
            ExitRectEditMode();

            //Get the next active rect in the hierarchy
            int nextActiveIndex = -1;
            for(int i = 0; i < rectTransforms.Count; i++)
            {
                int rectIndex = (currentRectIndex + 1 + i) % rectTransforms.Count;

                if(rectTransforms[rectIndex] != null)
                {
                    if (rectTransforms[rectIndex].parent.gameObject.activeInHierarchy)
                    {
                        nextActiveIndex = rectIndex;
                        break;
                    }
                }
            }

            if (nextActiveIndex != -1)
            {
                currentRectIndex = nextActiveIndex;
                EnterRectEditMode(rectTransforms[currentRectIndex]);
            }
            else
            {
                PlayerManager.Instance.messenger.Log("No configurable UI available.", 2f);
            }
        }

        private void CycleToPreviousRect()
        {
            if (rectTransforms.Count == 0) return;
            ExitRectEditMode();

            // Get the previous active rect in the hierarchy
            int previousActiveIndex = -1;
            for (int i = 0; i < rectTransforms.Count; i++)
            {
                int rectIndex = (currentRectIndex - 1 - i + rectTransforms.Count) % rectTransforms.Count;

                if (rectTransforms[rectIndex] != null)
                {
                    if (rectTransforms[rectIndex].parent.gameObject.activeInHierarchy)
                    {
                        previousActiveIndex = rectIndex;
                        break;
                    }
                }
            }

            if (previousActiveIndex != -1)
            {
                currentRectIndex = previousActiveIndex;
                EnterRectEditMode(rectTransforms[currentRectIndex]);
            }
            else
            {
                PlayerManager.Instance.messenger.Log("No configurable UI available.", 2f);
            }
        }

        private void HandleEditing()
        {
            if (currentRect == null) return;

            Vector2 mousePosition = Input.mousePosition;
            RectTransform rectTransform = currentRect;

            if (isMoving)
            {
                Vector2 delta = (mousePosition - initialMousePosition) / new Vector2(Screen.width, Screen.height);
                rectTransform.anchorMin = initialAnchorMin + delta;
                rectTransform.anchorMax = initialAnchorMax + delta;
                configManager.SetAnchors(rectTransform);
            }
            else if (isScaling)
            {
                Vector2 screenPosition = new Vector2(mousePosition.x / Screen.width, mousePosition.y / Screen.height);
                rectTransform.anchorMax = screenPosition;
                configManager.SetAnchorMax(rectTransform);
            }
        }

        private void StopEditing()
        {
            isMoving = false;
            isScaling = false;
        }

        private void EnterRectEditMode(RectTransform rect)
        {
            currentRect = rect;

            preEditRectActive = currentRect.gameObject.activeSelf;

            //If it has text content, check if its empty and get a placeholder text for that UI.
            TextMeshProUGUI textGUI = rect.GetComponent<TextMeshProUGUI>();
            if(textGUI != null)
            {
                string textGUIText = textGUI.text;
                if(textGUIText == "")
                {
                    //Get the placeholder
                    string placeHolderText = GetPlaceHolderText(rect);
                    setEditPlaceHolderText = true;
                    preEditTextContent = textGUIText;
                    textGUI.text = placeHolderText;
                }
            }

            currentRect.gameObject.SetActive(true);

            isRectEditing = true;
            ActivateHandles(rect);
            PlayerManager.Instance.messenger.Log(rect.gameObject.name, 1f);
        }
        
        private void ExitRectEditMode()
        {
            if(setEditPlaceHolderText)
            {
                TextMeshProUGUI textGUI = currentRect.GetComponent<TextMeshProUGUI>();
                if (textGUI != null)
                {
                    textGUI.text = preEditTextContent;
                }

                setEditPlaceHolderText = false;
            }

            currentRect.gameObject.SetActive(preEditRectActive);

            isRectEditing = false;
            DeactivateHandles();
            currentRect = null;
        }

        public void AddRectTransform(RectTransform rectTransform)
        {
            if (!rectTransforms.Contains(rectTransform))
            {
                rectTransforms.Add(rectTransform);
                configManager.RectAdded(rectTransform);                
            }
        }

        public void RemoveRectTransformHandler(RectTransformHandler handler)
        {
            rectTransforms.Remove(handler.GetComponent<RectTransform>());
        }
        
        public void OnGUI()
        {
            if (isEditing)
            {
                Rect screenborderRect = new Rect(0, 0, Screen.width, Screen.height);
                RectDrawUtils.DrawScreenRectBorder(screenborderRect, 8, animatedBorderColor);

                if(currentRect != null)
                {
                    //Draw borders
                    Vector3[] corners = new Vector3[4];
                    currentRect.GetWorldCorners(corners);

                    Canvas canvas = currentRect.GetComponentInParent<Canvas>();
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            corners[i] = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                        }
                    }
                    else
                    {
                        Camera camera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
                        for (int i = 0; i < 4; i++)
                        {
                            corners[i] = RectTransformUtility.WorldToScreenPoint(camera, corners[i]);
                        }
                    }

                    Rect screenRect = RectDrawUtils.GetScreenRect(corners[0], corners[2]);
                    RectDrawUtils.DrawScreenRectBorder(screenRect, 2, borderColor);
                }
            }
        }
        
        public void ResetAll()
        {
            foreach(RectTransform rect in rectTransforms)
            {
                configManager.ResetToOriginal(rect);
            }

            configManager.ResetAllSettings();
            configManager.SaveSettings();            
        }

        void StartMove(RectTransform rect)
        {
            currentRect = rect;
            isMoving = true;
            isScaling = false;
            initialMousePosition = Input.mousePosition;
            initialAnchorMin = currentRect.anchorMin;
            initialAnchorMax = currentRect.anchorMax;
        }

        void StartScale(RectTransform rect)
        {
            currentRect = rect;
            isScaling = true;
            isMoving = false;
            initialMousePosition = Input.mousePosition;
            initialAnchorMin = currentRect.anchorMin;
            initialAnchorMax = currentRect.anchorMax;
        }

        void ActivateHandles(RectTransform rect)
        {
            if (moveHandle == null)
            {
                moveHandle = CreateHandle("MoveHandle", StartMove);
            }

            if (scaleHandle == null)
            {
                scaleHandle = CreateHandle("ScaleHandle", StartScale);
            }

            if (resetHandle == null)
            {
                resetHandle = CreateHandle("ResetHandle", configManager.ResetToOriginal);
            }

            PositionHandle(moveHandle, rect, new Vector2(0, 0));
            PositionHandle(scaleHandle, rect, new Vector2(1, 1));
            PositionHandle(resetHandle, rect, new Vector2(0, 1));
        }

        void DeactivateHandles()
        {
            if (moveHandle != null) moveHandle.SetActive(false);
            if (scaleHandle != null) scaleHandle.SetActive(false);
            if (resetHandle != null) resetHandle.SetActive(false);
        }

        GameObject CreateHandle(string name, UnityEngine.Events.UnityAction<RectTransform> callback)
        {
            GameObject handle = new GameObject(name);
            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(32, 32);

            Image image = handle.AddComponent<Image>();
            image.color = Color.white;

            Sprite s = UIConfiguratorUtils.GetSprite(name);
            if(s != null)
            {
                image.sprite = s;
            }

            EventTrigger trigger = handle.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((data) => callback(currentRect));
            trigger.triggers.Add(entry);

            handle.SetActive(false);
            return handle;
        }

        void PositionHandle(GameObject handle, RectTransform parent, Vector2 anchor)
        {
            handle.transform.SetParent(parent, false);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = anchor;
            handleRect.anchorMax = anchor;
            handleRect.pivot = anchor;
            handleRect.anchoredPosition = Vector2.zero;
            handle.SetActive(true);
        }        
    
        private string GetPlaceHolderText(RectTransform rect)
        {
            string rectName = rect.name.ToLower();
            switch(rectName)
            {
                //Player screen
                case "timeout title":
                    return "Too Slow!\nPress Right Shift to quick reset!";
                case "timeout countdown":
                    return "X.X";
                case "results position":
                    return "Results Position Placeholder";
                case "results time":
                    return "XX:XX.XXX";
                case "results checkpoints":
                    return "X/X Checkpoints";
                case "results press to continue":
                    return "Press Right Shift to continue!";
                case "velocity":
                    return "888";
                case "checkpoints":
                    return "X/X";
                case "time":
                    return "XX:XX.XXX";

                //Photomode
                case "target":
                    return "Target: Player 1";
                case "mode":
                    return "Mode: Free";
                case "fov":
                    return "60°";
                case "level":
                    return "Level X by Bouwerman";
                case "time left holder":
                    return "XX:XX";
                case "tooltips":
                    return "F1: Show Controls\nF2: Toggle Camera GUI\nF3: Toggle Small Leaderboard";
                case "velocitypanel":
                    return "XXX - XX:XX.XXX";
                case "target steam id":
                    return "Steam ID: XXXXXXXXXXXXXXXXXX";
                case "draw players indicator":
                    return "Show Player: state (V)";

                //Online
                case "voteskip text":
                case "voteskip text alternate position":
                    return "voteskip (X/X)";
                case "servermessage":
                case "servermessage alternate position":
                    return "Placeholder text for server message.";
            }

            return "";
        }    
    }    
}