#if UIWIDGETS_DATABIND_SUPPORT
namespace UIWidgets.DataBind
{
	using Slash.Unity.DataBind.Foundation.Observers;

	/// <summary>
	/// Observes value changes of the SelectedItem of an ListViewHeight.
	/// </summary>
	public class ListViewHeightSelectedItemObserver : ComponentDataObserver<UIWidgets.ListViewHeight, System.String>
	{
		/// <inheritdoc />
		protected override void AddListener(UIWidgets.ListViewHeight target)
		{
			target.OnSelectString.AddListener(OnSelectStringListViewHeight);
			target.OnDeselectString.AddListener(OnDeselectStringListViewHeight);
		}

		/// <inheritdoc />
		protected override System.String GetValue(UIWidgets.ListViewHeight target)
		{
			return target.SelectedItem;
		}
		
		/// <inheritdoc />
		protected override void RemoveListener(UIWidgets.ListViewHeight target)
		{
			target.OnSelectString.RemoveListener(OnSelectStringListViewHeight);
			target.OnDeselectString.RemoveListener(OnDeselectStringListViewHeight);
		}
		
		void OnSelectStringListViewHeight(System.Int32 arg0, System.String arg1)
		{
			OnTargetValueChanged();
		}

		void OnDeselectStringListViewHeight(System.Int32 arg0, System.String arg1)
		{
			OnTargetValueChanged();
		}
	}
}
#endif