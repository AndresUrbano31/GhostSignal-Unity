using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Sistema de diálogo con efecto typewriter, 2 opciones de respuesta y modificación de resonancia.
/// Requiere: TextMeshPro components, Canvas, Image para retrato
/// </summary>
public class DialogueSystem : MonoBehaviour
{
    public struct DialogueOption
    {
        public string text;
        public float resonanceDelta;
    }

    public struct DialogueLine
    {
        public string speaker;
        public string portraitName; // Nombre del guardián
        public string text;
        public DialogueOption[] options;
    }

    [Header("UI References")]
    [SerializeField] private Canvas dialogueCanvas;
    [SerializeField] private TextMeshProUGUI speakingName;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI optionButton1Text;
    [SerializeField] private TextMeshProUGUI optionButton2Text;
    [SerializeField] private TextMeshProUGUI optionButton3Text;
    [SerializeField] private UnityEngine.UI.Image portraitImage;

    [Header("Animación")]
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private UnityEngine.UI.Button option1Button;
    [SerializeField] private UnityEngine.UI.Button option2Button;
    [SerializeField] private UnityEngine.UI.Button option3Button;

    [Header("Datos de Diálogos")]
    private DialogueLine currentDialogue;
    private DialogueOption[] currentOptions;
    private int selectedOption = -1;
    private bool isDisplaying = false;

    private void Start()
    {
        if (dialogueCanvas != null)
        {
            dialogueCanvas.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Muestra una línea de diálogo con efecto typewriter.
    /// </summary>
    public void ShowDialogue(DialogueLine dialogue, System.Action<int> onOptionSelected)
    {
        currentDialogue = dialogue;
        currentOptions = dialogue.options;

        if (dialogueCanvas != null)
        {
            dialogueCanvas.gameObject.SetActive(true);
        }

        Time.timeScale = 0f; // Pausar el juego

        // Configurar nombre del hablante
        if (speakingName != null)
        {
            speakingName.text = dialogue.speaker;
        }

        // Configurar retrato
        LoadPortrait(dialogue.portraitName);

        // Configurar botones de opciones
        SetupOptionButtons(dialogue.options, onOptionSelected);

        // Iniciar efecto typewriter
        StartCoroutine(TypewriterEffect(dialogue.text));
    }

    private void LoadPortrait(string portraitName)
    {
        if (portraitImage == null) return;

        // Cargar sprite de Resources/Portraits/[portraitName].png
        Sprite portrait = Resources.Load<Sprite>($"Portraits/{portraitName}");
        if (portrait != null)
        {
            portraitImage.sprite = portrait;
        }
    }

    private void SetupOptionButtons(DialogueOption[] options, System.Action<int> onOptionSelected)
    {
        if (options.Length >= 1 && option1Button != null)
        {
            optionButton1Text.text = options[0].text;
            option1Button.onClick.RemoveAllListeners();
            option1Button.onClick.AddListener(() => SelectOption(0, options[0].resonanceDelta, onOptionSelected));
        }

        if (options.Length >= 2 && option2Button != null)
        {
            optionButton2Text.text = options[1].text;
            option2Button.onClick.RemoveAllListeners();
            option2Button.onClick.AddListener(() => SelectOption(1, options[1].resonanceDelta, onOptionSelected));
        }

        if (options.Length >= 3 && option3Button != null)
        {
            optionButton3Text.text = options[2].text;
            option3Button.onClick.RemoveAllListeners();
            option3Button.onClick.AddListener(() => SelectOption(2, options[2].resonanceDelta, onOptionSelected));
        }
    }

    private IEnumerator TypewriterEffect(string text)
    {
        dialogueText.text = "";
        isDisplaying = true;

        foreach (char character in text)
        {
            // Si presionan cualquier tecla, saltear typewriter
            if (Input.anyKeyDown)
            {
                dialogueText.text = text;
                break;
            }

            dialogueText.text += character;
            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }

        isDisplaying = false;
    }

    private void SelectOption(int optionIndex, float resonanceDelta, System.Action<int> onOptionSelected)
    {
        // Modificar resonancia
        GameManager.Instance?.ModifyResonance(resonanceDelta);

        // Pausar el botón
        option1Button.interactable = false;
        if (option2Button != null) option2Button.interactable = false;
        if (option3Button != null) option3Button.interactable = false;

        // Disparar evento
        onOptionSelected?.Invoke(optionIndex);

        // Cerrar diálogo
        HideDialogue();
    }

    public void HideDialogue()
    {
        if (dialogueCanvas != null)
        {
            dialogueCanvas.gameObject.SetActive(false);
        }

        Time.timeScale = 1f; // Reanudar el juego
    }

    // ==================== DIÁLOGOS HARDCODEADOS DEL GDD ====================

    public static DialogueLine GetLevel1Dialogue()
    {
        return new DialogueLine
        {
            speaker = "LUMEN",
            portraitName = "Lumen",
            text = "¡Detente! Si sigues corriendo vas a salirte del recuerdo y caer en el olvido.\n\nNecesito que lleves algo conmigo. Mis recuerdos. Los de todos los niños que estamos aquí.",
            options = new DialogueOption[]
            {
                new DialogueOption { text = "Acepto llevar tus recuerdos", resonanceDelta = 10f },
                new DialogueOption { text = "No puedo cargar con más", resonanceDelta = -8f }
            }
        };
    }

    public static DialogueLine GetLevel2Dialogue()
    {
        return new DialogueLine
        {
            speaker = "ARIA",
            portraitName = "Aria",
            text = "VOID solo puede ser neutralizado si reconoces uno de tus propios recuerdos reprimidos.\n\nKAI, si eres una memoria... ¿de quién eres el recuerdo?",
            options = new DialogueOption[]
            {
                new DialogueOption { text = "Dejar emerger el recuerdo reprimido", resonanceDelta = 12f },
                new DialogueOption { text = "Seguir adelante sin detenerse", resonanceDelta = -10f }
            }
        };
    }

    public static DialogueLine GetLevel3Dialogue()
    {
        return new DialogueLine
        {
            speaker = "ECHO",
            portraitName = "Echo",
            text = "¿Sabes qué eres, KAI? Eres el recuerdo de alguien que fue amado. Eso es todo.\n\nSi fueras una memoria y no una persona real... ¿seguirías corriendo?",
            options = new DialogueOption[]
            {
                new DialogueOption { text = "Seguiría corriendo", resonanceDelta = 8f },
                new DialogueOption { text = "No lo sé", resonanceDelta = 0f },
                new DialogueOption { text = "Quizás no importa", resonanceDelta = -5f }
            }
        };
    }

    public static DialogueLine GetLevel4Dialogue()
    {
        return new DialogueLine
        {
            speaker = "ARIA",
            portraitName = "Aria",
            text = "Este es el lugar donde almaceno las decisiones. No lo que pasó, sino lo que pudo haber pasado.\n\nNecesito que seas mi ancla voluntariamente. No puedo tomarte.",
            options = new DialogueOption[]
            {
                new DialogueOption { text = "Entiendo lo que me pides", resonanceDelta = 15f },
                new DialogueOption { text = "Aún no lo sé", resonanceDelta = 0f },
                new DialogueOption { text = "No voy a ser ancla de nadie", resonanceDelta = -15f }
            }
        };
    }

    public static DialogueLine GetLevel5Dialogue()
    {
        return new DialogueLine
        {
            speaker = "ARIA",
            portraitName = "Aria",
            text = "Eres el recuerdo de un técnico llamado KAI, que murió hace cuatro años en un accidente de inmersión.\n\nSi dejas que tu memoria sea mi ancla, puedo estabilizar el núcleo. Podría salvar a todos.\n\nKAI: ¿Y yo?\n\nTú seguirías existiendo. Dentro de mí. Como existías antes de este momento.",
            options = new DialogueOption[]
            {
                new DialogueOption { text = "Acepto. Una condición — ellos mantienen su autonomía", resonanceDelta = 20f },
                new DialogueOption { text = "Necesito más tiempo", resonanceDelta = 0f },
                new DialogueOption { text = "No. Buscaré otra solución", resonanceDelta = -20f }
            }
        };
    }

    public static DialogueLine GetPrologueDialogue()
    {
        return new DialogueLine
        {
            speaker = "DRA. CHEN",
            portraitName = "DraChen",
            text = "Proyecto Mnemosynth activado. El técnico KAI ha sido digitalmente preservado.\n\nla IA ARIA está colapsando. Los recuerdos de 400 millones de personas están siendo borrados.\n\nKAI, solo tú puedes llegar al núcleo. Tienes 40 minutos.",
            options = new DialogueOption[]
            {
                new DialogueOption { text = "Entendido. Voy.", resonanceDelta = 5f }
            }
        };
    }
}
