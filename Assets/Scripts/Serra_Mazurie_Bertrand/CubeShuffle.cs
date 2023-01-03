using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CubeShuffle : MonoBehaviour
{
    [SerializeField] private float shuffleSpeed = 500;

    public  bool            shuffling { get; private set; } = false;
    private CubeCreator     cubeCreator;
    private CubeRotation    cubeRotation;
    private CubeResolve     cubeResolve;
    
    private TextMeshProUGUI shufflingText;
    private Button          shuffleButton;
    private Slider          shuffleSlider;
    private TextMeshProUGUI stepsCountText;

    void Awake()
    {
        cubeCreator    = gameObject.GetComponent<CubeCreator >();
        cubeRotation   = gameObject.GetComponent<CubeRotation>();
        cubeResolve    = gameObject.GetComponent<CubeResolve >();
        shufflingText  = GameObject.Find("ShufflingText" ).GetComponent<TextMeshProUGUI>();
        shuffleButton  = GameObject.Find("ShuffleButton" ).GetComponent<Button>();
        shuffleSlider  = GameObject.Find("ShuffleSlider" ).GetComponent<Slider>();
        stepsCountText = GameObject.Find("StepsCountText").GetComponent<TextMeshProUGUI>();

        shuffleButton.onClick.AddListener(StartShuffle);
        shuffleSlider.onValueChanged.AddListener(UpdateStepsCountText);
    }

    private void UpdateStepsCountText(float value)
    {
        stepsCountText.SetText((value * 10).ToString());
    }

    private IEnumerator ShuffleCube()
    {
        shuffling = true;
        float cubeRotationSpeed = cubeRotation.rotationSpeed;
        cubeRotation.rotationSpeed = shuffleSpeed;
        StartCoroutine(Extensions.FadeText(shufflingText, 0.8f, true));

        for (int i = 0; i < shuffleSlider.value * 10; i++)
        {
            // Wait for the cube to have finished rotating.
            while (cubeRotation.rotating)
                yield return null;

            // Get a random normal along which it will rotate.
            Vector3 rotAxis = Vector3.zero;
            switch (Random.Range(0, 3))
            {
                case 0:  rotAxis = Vector3.up;      break;
                case 1:  rotAxis = Vector3.right;   break;
                default: rotAxis = Vector3.forward; break;
            }

            // Get a random slice of the cube to be rotated.
            int rotIndex = Random.Range(0, cubeCreator.size);

            // Randomly choose wether or not to reverse the rotation.
            bool rotReverse = Random.Range(0, 2) == 1;

            // Don't undo the previous rotation.
            if (rotAxis == cubeRotation.rotationAxis && rotIndex == cubeRotation.rotationSlice && rotReverse != cubeRotation.rotationReverse) {
                i--;
                continue;
            }

            // Rotate the slice.
            cubeRotation.RotateSlice(rotAxis, rotIndex, rotReverse);
        }

        StartCoroutine(Extensions.FadeText(shufflingText, 0.8f, false));
        cubeRotation.rotationSpeed = cubeRotationSpeed;
        shuffling = false;
    }

    public void StartShuffle()
    {
        if (!shuffling && !cubeResolve.resolving)
            StartCoroutine(ShuffleCube());
    }

    public void StopShuffle()
    {
        if (shuffling) {
            shuffling = false;
            StopAllCoroutines();
            StartCoroutine(Extensions.FadeText(shufflingText, 0.8f, false));
        }
    }
}
