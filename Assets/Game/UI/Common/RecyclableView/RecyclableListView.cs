using UnityEngine;

namespace Game.UI
{
    public abstract class RecyclableListView : MonoBehaviour
    {
        [SerializeField]
        protected GameObject viewPrefab;

        [SerializeField]
        protected RecyclableScrollRect scrollRect;
        
        public abstract void Initialize(IAdapter adapter);

        public abstract void Terminate();
        
        public interface IAdapter
        {
            int GetDataCount();

            void OnCreateView(RectTransform view, int index);

            void OnUpdateView(RectTransform view, int index);

            void OnDestroyView(RectTransform view);
        }
    }
}