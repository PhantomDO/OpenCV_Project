using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Ocl;
using Emgu.CV.Util;
using UnityEngine;

namespace TD3
{
    public class FaceDetection : MonoBehaviour
    {
        [UnityEngine.SerializeField] private UnityEngine.UI.RawImage _rawImage;
        [SerializeReference] private VideoCapture _webcam;

        private Image<Bgr, byte> _currentFrame;

        // Start is called before the first frame update
        void Start()
        {
            _webcam = new VideoCapture();

            _webcam.Start();   
            _webcam.ImageGrabbed += HandleWebcamQueryFrame;
        }

        // Update is called once per frame
        void Update()
        {
            if (_webcam.IsOpened){
                _webcam.Grab();
                DisplayFrameOnPlane();
            }
            else{
                Debug.LogWarning($"{_webcam} is not open.");
            }
        }

        // OnDestroy is called at the end of the object (when Destroy(gameObject) is called)
        void OnDestroy()
        {
            _webcam.ImageGrabbed -= HandleWebcamQueryFrame;
            _webcam.Stop();
            _webcam.Dispose();
        }

        // HandleWebcamQueryFrame is called when acquisition event occurs.
        // Get the current frame and face detection
        void HandleWebcamQueryFrame(object sender, System.EventArgs eventArgs)
        {
            Mat mat = new Emgu.CV.Mat();
            if (((VideoCapture)sender).Retrieve(mat)){
                _currentFrame = mat.ToImage<Bgr, byte>();
                Debug.Log($"Size of image: {_currentFrame.Size}");
            }
            else {
                Debug.LogError($"No img retrived.");
            }
        }

        // DisplayFrameOnPlane
        // Load the current frame in a texture draw on plane
        void DisplayFrameOnPlane()
        {
            Texture2D tex = new Texture2D(_rawImage.mainTexture.width, _rawImage.mainTexture.height);
            _currentFrame.Resize(_rawImage.mainTexture.width, _rawImage.mainTexture.height, Inter.Linear);
            var _currendFrameRGBA = new Mat(_rawImage.mainTexture.width, _rawImage.mainTexture.height, DepthType.Cv32S, 4);
            CvInvoke.CvtColor(_currentFrame, _currendFrameRGBA, ColorConversion.Bgr2Rgba);

            tex.LoadImage(_currentFrame.ToTexture2D().GetRawTextureData(), false);
            tex.Apply();

            _rawImage.texture = tex;
        }


    }
}
