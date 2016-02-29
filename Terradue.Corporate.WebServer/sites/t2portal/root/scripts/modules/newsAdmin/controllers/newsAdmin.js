
define([
	'jquery',
	'can',
	'bootbox',
	'utils/crudBaseModule/crudBaseControl',
	'config',
	'utils/helpers',
	'modules/newsAdmin/models/newsAdmin',
	'messenger',
	'summernote'
], function($, can, bootbox, CrudBaseControl, Config, Helpers, NewsModel){
	
	var NewsAdminControl = CrudBaseControl({}, {
		
		onIndex: function(element, options){
			var self = this;
		},
		
		onEntitySelected: function(news){
			Helpers.scrollTop();
			
			// destroy eventual previous summernote
			this.element.find('textarea[name="Content"]').summernote('destroy');
			
			// init summernote
			this.element.find('textarea[name="Content"]').summernote();
		},
		
		onCreateClick: function(){
			Helpers.scrollTop();
			this.element.find('textarea[name="Content"]').summernote('reset');
		},
		
	});
	
	return new NewsAdminControl(Config.mainContainer, {
		Model: NewsModel,
		entityName: 'news',
		view: '/scripts/modules/newsAdmin/views/newsAdmin.html',
		insertAtBeginning: true,
		loginDeferred: App.Login.isLoggedDeferred,
		adminAccess: true
	});
	
});
