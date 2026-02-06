using Unity.GraphToolkit.Editor;
using Unity.GraphToolkit.Editor.Implementation;
using UnityEngine;
using UnityEngine.UIElements;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Displays a noise texture preview at the bottom of an FN2 node.
	/// Calls <see cref="FN2BridgeCallbacks.RenderNodePreview"/> to compile
	/// the subtree and render noise output. Caches the texture and debounces
	/// re-renders to avoid excessive computation.
	/// </summary>
	class FN2TexturePreviewPart : BaseModelViewPart
	{
		public static readonly string ussClassName = "fn2-texture-preview-part";
		const int PreviewSize = 128;
		const long DebounceMs = 200;

		VisualElement m_Root;
		Image m_Image;
		Texture2D m_CachedTexture;
		IVisualElementScheduledItem m_PendingRender;

		public static FN2TexturePreviewPart Create(string name, Model model,
			ChildView ownerElement, string parentClassName)
		{
			return new FN2TexturePreviewPart(name, model, ownerElement, parentClassName);
		}

		FN2TexturePreviewPart(string name, Model model, ChildView ownerElement,
			string parentClassName)
			: base(name, model, ownerElement, parentClassName) { }

		public override VisualElement Root => m_Root;

		protected override void BuildUI(VisualElement parent)
		{
			m_Root = new VisualElement();
			m_Root.AddToClassList(ussClassName);

			m_Image = new Image();
			m_Image.AddToClassList("fn2-preview-image");
			m_Root.Add(m_Image);

			parent.Add(m_Root);
		}

		public override void UpdateUIFromModel(UpdateFromModelVisitor visitor)
		{
			if (FN2BridgeCallbacks.RenderNodePreview == null)
				return;

			// Debounce: cancel pending render and schedule a new one
			m_PendingRender?.Pause();
			m_PendingRender = m_Root.schedule.Execute(RenderPreview);
			m_PendingRender.ExecuteLater(DebounceMs);
		}

		void RenderPreview()
		{
			if (FN2BridgeCallbacks.RenderNodePreview == null)
				return;

			var nodeModel = m_Model as IUserNodeModelImp;
			if (nodeModel == null)
				return;

			var newTexture = FN2BridgeCallbacks.RenderNodePreview(
				nodeModel.Node, PreviewSize, PreviewSize);

			if (m_CachedTexture != null && m_CachedTexture != newTexture)
				Object.DestroyImmediate(m_CachedTexture);

			m_CachedTexture = newTexture;
			m_Image.image = m_CachedTexture;
		}
	}
}
