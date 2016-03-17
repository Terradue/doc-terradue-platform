
define([
	'can',
	'config',
	'utils/baseControl',
	'modules/people/models/peopleData', // data loaded by require.js
	'bootbox',
	'utils/helpers',
	'modules/pages/controllers/pages'

], function(can, Config, BaseControl, peopleData, bootbox, Helpers, Pages){
	
	var SamplePageControl = BaseControl(
		{ defaults: { fade: 'slow' } }, // if you want a page transition effect
		{
			// init
			init: function (element, options) {
			},
			
			// first level page : /people
			index: function (options, data) {
				// view is an utility method
				Pages.view({
					url: 'modules/samplepage/views/samplepage.html',
					selector: Config.mainContainer
				});
			},
			
		}
	);
	
	return new SamplePageControl(Config.mainContainer, {});
	
});
