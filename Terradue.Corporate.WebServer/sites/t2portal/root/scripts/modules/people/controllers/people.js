
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
			

			
			// custom subpage sample (third level page: /people/details/<id>)
			details: function(options){
				// the pattern of each route url is <controller>/<action>/<id>
				// in this case you have
				// - options.controller = 'people'
				// - options.action = 'details'
				// - options.id = the t2 people id choosen
				console.log(options);

				// get the complete name (name.surname)
				var completeName = options.id;
				
				// search the user inside the data
				var search = $.grep(peopleData.list, function(p){
					return (p.surname+'.'+p.name == completeName);
				});
				if (search[0]) // if found
					this.view({
						url: 'modules/people/views/peopleDetails.html',
						data: search[0],
					});
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
