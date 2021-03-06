#if UIWIDGETS_DATABIND_SUPPORT
namespace UIWidgets.DataBind
{
	using Slash.Unity.DataBind.Foundation.Observers;

	/// <summary>
	/// Observes value changes of the Value of an CenteredSlider.
	/// </summary>
	public class CenteredSliderValueObserver : ComponentDataObserver<UIWidgets.CenteredSlider, System.Int32>
	{
		/// <inheritdoc />
		protected override void AddListener(UIWidgets.CenteredSlider target)
		{
			target.OnValuesChange.AddListener(OnValuesChangeCenteredSlider);
		}

		/// <inheritdoc />
		protected override System.Int32 GetValue(UIWidgets.CenteredSlider target)
		{
			return target.Value;
		}
		
		/// <inheritdoc />
		protected override void RemoveListener(UIWidgets.CenteredSlider target)
		{
			target.OnValuesChange.RemoveListener(OnValuesChangeCenteredSlider);
		}
		
		void OnValuesChangeCenteredSlider(System.Int32 arg0)
		{
			OnTargetValueChanged();
		}
	}
}
#endif