using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGame.HotFix.SceneFlow
{
    public sealed class LoadingScreenService : MonoBehaviour
    {
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TextMeshProUGUI _progressText;

        private CancellationTokenSource _fakeProgressCts;

        private void Awake()
        {
            _loadingPanel.SetActive(false);
            DontDestroyOnLoad(_loadingPanel);
        }

        public void Show(string message = "Loading...")
        {
            _fakeProgressCts?.Cancel();
            _fakeProgressCts = new CancellationTokenSource();

            _loadingPanel.SetActive(true);
            SetProgress(0f);

            RunFakeProgressAsync(_fakeProgressCts.Token).Forget();
        }

        public async UniTask HideAsync()
        {
            _fakeProgressCts?.Cancel();
            _fakeProgressCts = null;

            SetProgress(1f);

            await UniTask.Delay(200);

            _loadingPanel.SetActive(false);
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (_progressSlider != null)
            {
                _progressSlider.value = progress;
            }

            if (_progressText != null)
            {
                _progressText.text = $"{Mathf.CeilToInt(progress * 100)}%";
            }
        }

        private async UniTaskVoid RunFakeProgressAsync(CancellationToken ct)
        {
            float progress = 0f;

            while (progress < 0.9f && !ct.IsCancellationRequested)
            {
                progress += Random.Range(0.01f, 0.05f);
                SetProgress(progress);

                int delayMs = Random.Range(50, 180);
                await UniTask.Delay(delayMs, cancellationToken: ct);
            }
        }
    }
}