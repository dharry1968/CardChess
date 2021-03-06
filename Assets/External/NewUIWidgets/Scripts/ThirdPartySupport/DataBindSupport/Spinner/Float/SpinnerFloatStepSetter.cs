#if UIWIDGETS_DATABIND_SUPPORT
namespace UIWidgets.DataBind
{
	using Slash.Unity.DataBind.Foundation.Setters;
	using UnityEngine;
	
	/// <summary>
	/// Set the Step of a SpinnerFloat depending on the System.Single data value.
	/// </summary>
	[AddComponentMenu("Data Bind/New UI Widgets/Setters/[DB] SpinnerFloat Step Setter")]
	public class SpinnerFloatStepSetter : ComponentSingleSetter<UIWidgets.SpinnerFloat, System.Single>
	{
		/// <inheritdoc />
		protected override void UpdateTargetValue(UIWidgets.SpinnerFloat target, System.Single value)
		{
			target.Step = value;
		}
	}
}
#endif