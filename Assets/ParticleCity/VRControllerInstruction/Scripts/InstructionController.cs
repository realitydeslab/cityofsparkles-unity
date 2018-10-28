using JetBrains.Annotations;
using UnityEngine;
using TMPro;
using WanderUtils;

public class InstructionController : MonoBehaviour
{
    public bool TutorialEnabled = true;

    [CanBeNull] public Transform ArrowTarget;
    public AnimationCurve ArrowCurrentAlphaOverArrowAngle;
    public AnimationCurve ArrowTargetAlphaOverArrowAngle;

    [SerializeField]
    private GameObject menu;

    [SerializeField]
    private GameObject trigger;

    [SerializeField]
    private GameObject gripLeft;

    [SerializeField]
    private GameObject gripRight;

    [SerializeField]
    private GameObject touchPan;

    [SerializeField]
    private GameObject arrowCurrent;

    [SerializeField]
    private GameObject arrowTarget;

    [SerializeField]
    private GameObject rotate;

    private TextMeshPro menuText;
    private TextMeshPro triggerText;
    private TextMeshPro gripLeftText;
    private TextMeshPro gripRightText;
    private TextMeshPro touchPanText;
    private TextMeshPro rotateText;

    public void Awake() {
        menuText = menu.GetComponentInChildren<TextMeshPro>();
        triggerText = trigger.GetComponentInChildren<TextMeshPro>();
        gripLeftText = gripLeft.GetComponentInChildren<TextMeshPro>();
        gripRightText = gripRight.GetComponentInChildren<TextMeshPro>();
        touchPanText = touchPan.GetComponentInChildren<TextMeshPro>();
        rotateText = rotate.GetComponentInChildren<TextMeshPro>();

        MenuText = "";
        TriggerText = "Trigger";
        GripText = "Grip";
        TouchPanText = "";
        RotateText = "";
    }

    public void Start() {
    }

    public void Update()
    {
        bool tutorialEnabled = TutorialEnabled;

        menu.SetActive(tutorialEnabled && !string.IsNullOrEmpty(MenuText));
        trigger.SetActive(tutorialEnabled && !string.IsNullOrEmpty(TriggerText));
        gripLeft.SetActive(tutorialEnabled && !string.IsNullOrEmpty(GripText));
        // gripRight.SetActive(tutorialEnabled && !string.IsNullOrEmpty(GripText));
        touchPan.SetActive(tutorialEnabled && !string.IsNullOrEmpty(TouchPanText));

        updateArrows();
    }

    public string MenuText {
        get {
            return menuText.text;
        }

        set {
            if (MenuText == value) {
                return;
            }
            menuText.text = value;
            menu.SetActive(TutorialEnabled && !string.IsNullOrEmpty(value));
        }
    }

    public string TriggerText {
        get {
            return triggerText.text;
        }

        set {
            if (TriggerText == value) {
                return;
            }
            triggerText.text = value;
            trigger.SetActive(TutorialEnabled && !string.IsNullOrEmpty(value));
        }
    }

    public string GripText {
        get {
            return gripLeftText.text;
        }

        set {
            if (GripText == value) {
                return;
            }
            gripLeftText.text = value;
            // gripRightText.text = value;
            gripLeft.SetActive(TutorialEnabled && !string.IsNullOrEmpty(value));
            // gripRight.SetActive(TutorialEnabled && !string.IsNullOrEmpty(value));
        }
    }

    public string TouchPanText {
        get {
            return touchPanText.text;
        }

        set {
            if (TouchPanText == value) {
                return;
            }

            touchPanText.text = value;
            touchPan.SetActive(TutorialEnabled && !string.IsNullOrEmpty(value)); 
        }
    }

    public string RotateText
    {
        get { return rotateText.text; }
        set
        {
            if (RotateText == value)
            {
                return;
            }

            rotateText.text = value;
            rotate.SetActive(TutorialEnabled && !string.IsNullOrEmpty(value));
        }
    }

    private void updateArrows()
    {
        if (TutorialEnabled && ArrowTarget != null)
        {
            arrowCurrent.SetActive(true);
            arrowTarget.SetActive(true);

            arrowTarget.transform.forward = (ArrowTarget.transform.position - arrowTarget.transform.position).normalized;

            float angle = Vector3.Angle(arrowCurrent.transform.forward, arrowTarget.transform.forward);
            arrowCurrent.GetComponentInChildren<Renderer>().material.color = Color.white.ColorWithAlpha(ArrowCurrentAlphaOverArrowAngle.Evaluate(angle));
            arrowTarget.GetComponentInChildren<Renderer>().material.color = Color.white.ColorWithAlpha(ArrowTargetAlphaOverArrowAngle.Evaluate(angle));
        }
        else
        {
            arrowCurrent.SetActive(false);
            arrowTarget.SetActive(false);
        }
    }
}
