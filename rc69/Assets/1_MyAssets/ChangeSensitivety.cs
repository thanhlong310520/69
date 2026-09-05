using Raccoon.Controller;
using TMPro;
using UnityEngine;

public class ChangeSensitivety : MonoBehaviour
{
    public CameraController cameraController;
    public UIInputHandler inputHandler;
    [Header("UI")]
    public TMP_InputField sensitivityInput;
    public TMP_InputField rotateSpeed;
    public TMP_InputField smooth;
    public TMP_Text showValue;

    float xRotation;

    private void Start()
    {
        // Hiển thị giá trị mặc định
        sensitivityInput.text = cameraController.cameraXSpeed.ToString();
        rotateSpeed.text = inputHandler.rotationSpeed.ToString();
        smooth.text = cameraController.cameraZoomDuration.ToString();

        // Lắng nghe khi nhập xong
        sensitivityInput.onEndEdit.AddListener(ChangeSensitivity);
        rotateSpeed.onEndEdit.AddListener(ChangeRotateSpeed);
        smooth.onEndEdit.AddListener(ChangeSmooth);
    }

    void ChangeSensitivity(string value)
    {  
        if (float.TryParse(value, out float result))
        {
            cameraController.cameraXSpeed = Mathf.Clamp(result / 10, .1f, 1000f);
            cameraController.cameraYSpeed = Mathf.Clamp(result / 10, .1f, 1000f);
        }
    }

    public void OnChangeValue(float value)
    {
        cameraController.targetOffset = new Vector3(0,1 + value ,0);
        showValue.text = " " + (1 + value);
    }

    void ChangeRotateSpeed(string value)
    {
        if (float.TryParse(value, out float result))
        {
            print("result: " + result);

            inputHandler.rotationSpeed = Mathf.Clamp(result / 10, 0.1f, 1000f);
        }
    }

    void ChangeSmooth(string value)
    {
        print("chamge rotate speed: " + value);
        if (float.TryParse(value, out float result))
        {
            print("result: " + result);

            cameraController.cameraZoomDuration = Mathf.Clamp(result, 0.1f, 1000f);
        }
    }

}
