using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Ocl;
using Emgu.CV.Util;
using UnityEngine;
using UnityEngine.UI;

namespace TD3
{
    public class FaceDetection : MonoBehaviour
    {
        [SerializeField] private bool _useVideo;
        [SerializeField] private int _videoIndex = 1;
        [SerializeField] private float _secondBetweenFrame = 0.2f;
        [SerializeField] private UnityEngine.UI.RawImage _rawImage;

        private VideoCapture _webcam;
        private CascadeClassifier _cascade;

        private Image<Bgr, byte> _currentFrameBgr;
        private Image<Rgb, byte> _currentFrameRgb;
        private Image<Bgra, byte> _currentFrameRgba;

        private bool _hasGrabImage; 
        private Vector2Int _frameSize;
        private Texture2D _texture2D;

        private WaitForSeconds _waitForSecond;

        // Start is called before the first frame update
        void Start()
        {
            _waitForSecond = new WaitForSeconds(_secondBetweenFrame);
            var devices = WebCamTexture.devices;
            for (int i = 0; i < devices.Length; i++)
            {
                Debug.Log($"Devices[{i}]: {devices[i].name}");
            }

            var index = devices.Length < _videoIndex ? devices.Length - 1 : _videoIndex;
            var videoPath = $"{Application.dataPath}{@"/Videos/demo.mp4"}";
            Debug.Log(videoPath);
            _webcam = _useVideo ? new VideoCapture(videoPath) : new VideoCapture(index);


            if (_webcam != null)
            {
                _webcam.ImageGrabbed += HandleWebcamQueryFrame;
                _webcam.Start();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!_webcam.IsOpened)
            {
                Debug.LogWarning($"{_webcam} is not open.");
            }
            else
            {
                StartCoroutine(GrabImage());
            }
        }

        // OnDestroy is called at the end of the object (when Destroy(gameObject) is called)
        void OnDestroy()
        {
            _webcam.ImageGrabbed -= HandleWebcamQueryFrame;
            _webcam.Stop();
            _webcam.Dispose();
        }

        IEnumerator GrabImage()
        {
            _webcam.Grab();
            if (_hasGrabImage)
            {
                DisplayFrameOnPlane();
            }
            else
            {
                Debug.LogError($"Can't grab image.");
            }

            yield return _waitForSecond;
        }

        // HandleWebcamQueryFrame is called when acquisition event occurs.
        // Get the current frame and face detection
        void HandleWebcamQueryFrame(object sender, System.EventArgs eventArgs)
        {
            if (_currentFrameBgr == null)
            {
                _currentFrameBgr = new Image<Bgr, byte>(_webcam.Width, _webcam.Height);
            }

            _hasGrabImage = _webcam.Retrieve(_currentFrameBgr);
            //_currentFrameBgr.Flip(FlipType.Vertical);

            if (_frameSize.x != _currentFrameBgr.Width || _frameSize.y != _currentFrameBgr.Height)
            {
                _frameSize = new Vector2Int(_currentFrameBgr.Width, _currentFrameBgr.Height);
                _currentFrameRgb = new Image<Rgb, byte>(_frameSize.x, _frameSize.y);
                _currentFrameRgba = new Image<Bgra, byte>(_frameSize.x, _frameSize.y);
            }

            Debug.Log($"Size of image: {_currentFrameBgr.Size}");
        }

        // DisplayFrameOnPlane
        // Load the current frame in a texture draw on plane
        void DisplayFrameOnPlane()
        {
            if (_texture2D != null)
            {
                Destroy(_texture2D);
                _texture2D = null;
            }

            var sizeRect = Vector2Int.CeilToInt(_rawImage.rectTransform.rect.size);
            _texture2D = new Texture2D(sizeRect.x, sizeRect.y, TextureFormat.BGRA32, false);

            _currentFrameRgb.Bytes = _currentFrameBgr.Bytes;
            _currentFrameRgba = _currentFrameRgb.Convert<Bgra, byte>();
            CvInvoke.Flip(_currentFrameRgba, _currentFrameRgba, FlipType.Both);
            CvInvoke.Resize(_currentFrameRgba, _currentFrameRgba, new Size(sizeRect.x, sizeRect.y));
            _texture2D.LoadRawTextureData(_currentFrameRgba.Bytes);
            _texture2D.Apply();

            _rawImage.texture = _texture2D;
        }
    }
}
