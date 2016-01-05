
define([
	'can',
	'config',
	'utils/baseControl',
	'modules/people/models/peopleData', // data loaded by require.js
	'bootbox'
], function(can, Config, BaseControl, peopleData, bootbox){
	
	var PeopleControl = BaseControl(
		{ defaults: { fade: 'slow' } }, // if you want a page transition effect
		{
			// init
			init: function (element, options) {
				// all you want to init, i.e. some data
				
				// dynamic loading of css (this is to test)
				this.loadCSS('modules/people/css/people.css');
			},
			
			// first level page : /people
			index: function (options) {
				// view is an utility method
				this.view({
					url: 'modules/people/views/people.html',
					data: peopleData,
				});
				
				// you can also use directly canjs
				//this.element.html(can.view('modules/people/views/people.html', peopleData));
			},
			
			
			// sample event
			'ul.tags>li click': function(elem){
				bootbox.alert('You have clicked on '+elem.find('a').html());
				return false;
			}
			
		}
	);
	
	return new PeopleControl(Config.mainContainer, {});
	
});
