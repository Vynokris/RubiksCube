using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CubeCreator : MonoBehaviour
{
    [SerializeField] private GameObject subCubePrefab;

    public  int   size     { get; private set; } = 3;
    public  bool  isSolved { get; private set; } = false;
    private bool  wasSolved                      = false;
    private float solvedCheckInterval            = 0.2f;
    private float solvedCheckTimer               = 0f;
    private float defaultRotationSpeed           = 200;

    private CubeRotation cubeRotation;
    private CubeShuffle  cubeShuffle;
    private CubeResolve  cubeResolve;

    private TextMeshProUGUI solvedText;
    private Button          quitButton;
    private Button          resetButton;
    private Slider          sizeSlider;
    private TextMeshProUGUI sizeValueText;

    void Awake()
    {
        cubeRotation  = gameObject.GetComponent<CubeRotation>();
        cubeResolve   = gameObject.GetComponent<CubeResolve >();
        cubeShuffle   = gameObject.GetComponent<CubeShuffle >();
        solvedText    = GameObject.Find("SolvedText"   ).GetComponent<TextMeshProUGUI>();
        quitButton    = GameObject.Find("QuitButton"   ).GetComponent<Button>();
        resetButton   = GameObject.Find("ResetButton"  ).GetComponent<Button>();
        sizeSlider    = GameObject.Find("SizeSlider"   ).GetComponent<Slider>();
        sizeValueText = GameObject.Find("SizeValueText").GetComponent<TextMeshProUGUI>();

        defaultRotationSpeed = cubeRotation.rotationSpeed;

        quitButton.onClick.AddListener(Application.Quit);
        resetButton.onClick.AddListener(ResetCube);
        sizeSlider.onValueChanged.AddListener(UpdateCubeSize);

        Create();
    }

    private void UpdateCubeSize(float value)
    {
        sizeValueText.SetText(value.ToString());
        ResetCube();
    }

    private void Create()
    {
        // Create the rubik's cube parent.
        GameObject rubiksCube = new GameObject("Rubik's Cube (" + size.ToString() + "x" + size.ToString() + "x" + size.ToString() + ")");
        rubiksCube.transform.position = new Vector3(0, 0, 0);
        rubiksCube.transform.SetParent(transform, false);

        // Get the rubik's cube size from the size slider.
        size = (int)sizeSlider.value;

        // Compute the offset used to center the cube.
        Vector3 offset = new Vector3(size / 2, size / 2, size / 2);
        if (size % 2 == 0)
            offset -= new Vector3(0.5f, 0.5f, 0.5f);

        // Create all of the sub-cubes.
        for (int z = 0; z < size; ++z) {
            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    if (!(x == 0 || x == size-1 || y == 0 || y == size-1 || z == 0 || z == size-1))
                        continue;

                    GameObject curSubCube = Instantiate(subCubePrefab, new Vector3(x, y, z), Quaternion.identity);
                    curSubCube.name = "Sub-Cube (" + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + ")";
                    curSubCube.transform.position -= offset;
                    curSubCube.transform.SetParent(rubiksCube.transform, false);
                }
            }
        }

        // Keep the cube's size consistent.
        transform.localScale = new Vector3(3f / size, 3f / size, 3f / size) * 1000f;
    }

    private void ResetCube()
    {
        // Stop any actions happening on the cube.
        cubeShuffle.StopShuffle();
        cubeResolve.StopResolve();
        cubeResolve.ResetHistory();
        cubeRotation.StopRotation();
        cubeRotation.rotationSpeed = defaultRotationSpeed;

        // Destroy the previous cube and create a new one.
        GameObject.Destroy(transform.GetChild(0).gameObject);
        Create();
    }

    void Update()
    {
        if (solvedCheckTimer > solvedCheckInterval)
        {
            bool isSolved = true;
            Vector3 firstFwd = transform.GetChild(0).GetChild(0).forward;
            Vector3 firstUp  = transform.GetChild(0).GetChild(0).up;
            foreach (Transform child in transform.GetChild(0)) {
                if (child.forward != firstFwd || child.up != firstUp) {
                    isSolved = false;
                    break;
                }
            }

            if (isSolved && !wasSolved)
                StartCoroutine(Extensions.FadeText(solvedText, 0.8f, true));
            else if (!isSolved && wasSolved)
                StartCoroutine(Extensions.FadeText(solvedText, 0.8f, false));

            wasSolved = isSolved;
            solvedCheckTimer = 0f;
        }
        solvedCheckTimer += Time.deltaTime;
    }
}
