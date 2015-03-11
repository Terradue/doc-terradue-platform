define([
	'can',
	'config',
	'utils/baseControl',
	'modules/newpeoplesimple/models/peopleData', // data loaded by require.js
	'bootbox'
], function(can, Config, BaseControl, peopleData, bootbox){
	
	var PeopleControl = BaseControl(
		{ defaults: { fade: 'slow' } }, // if you want a page transition effect
		{
			// init
			init: function (element, options) { },
			
			// first level page : /newpeoplesimple
			index: function (options) {
				// view is an utility method
				this.view({
					url: 'modules/newpeoplesimple/views/people.html',
					data: peopleData,
				});
			},
		}
	);
	
	return new PeopleControl(Config.mainContainer, {});
	
});
