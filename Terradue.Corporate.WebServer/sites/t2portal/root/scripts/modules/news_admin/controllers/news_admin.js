
define([
	'jquery',
	'can',
	'bootbox',
	'utils/crudBaseModule/crudBaseControl',
	'config',
	'utils/helpers',
	'modules/news_admin/models/news_admin',
	'messenger',
	'summernote',
	'datePicker'
], function($, can, bootbox, CrudBaseControl, Config, Helpers, NewsModel){
	
	var NewsAdminControl = CrudBaseControl({}, {
		
		onIndex: function(element, options){
			var self = this;
		},
		
		onEntitiesLoaded: function(newses){
			$.each(newses, function(i, news){
				// set simpleContent
				if (news.Content.startsWith('<img class="preview" src="')){
					if (!news.imageUrl){
						var imgTag = news.Content.substring(0, news.Content.indexOf('/>')+2);
						var $img = $(imgTag);
						news.attr('imageUrl', $img.attr('src'));
						news.attr('simpleContent', news.Content.substring(imgTag.length));
					} 
				} else
					news.attr('simpleContent', news.Content);
			});
		},
		
		onEntitySelected: function(news){
			var self = this;
			Helpers.scrollTop();
			
			// destroy eventual previous summernote
			this.element.find('textarea[name="simpleContent"]').summernote('destroy');
			
			// init summernote
			this.element.find('textarea[name="simpleContent"]').summernote();
			
			this.element.find('input[name="Date"]').datepicker({
				format: "yyyy-mm-dd"
			});
		},
		
		onCreateClick: function(){
			Helpers.scrollTop();
			this.element.find('textarea[name="simpleContent"]').summernote('reset');
		},
		
		onBeforeSave: function(entity){
			entity.attr('Content', (entity.imageUrl ? '<img class="preview" src="' + entity.imageUrl + '" />' : '')
					+ entity.simpleContent);
		},
		
		'.setDateNow click': function(){
			this.element.find('input[name="Date"]').val((new Date()).toISOString());
		},
		
		'input[name="imageUrl"] keyup': function(el){
			var self = this;
			var imageUrl = el.val();
			
			clearTimeout(this.keyTimer);
			this.keyTimer = setTimeout(function(){
				if (imageUrl)
					self.element.find('img.imageUrl-preview').show().attr('src', imageUrl);
				else
					self.element.find('img.imageUrl-preview').hide();
			}, 500);
		}
		
	});
	
	return new NewsAdminControl(Config.mainContainer, {
		Model: NewsModel,
		entityName: 'news',
		view: '/scripts/modules/news_admin/views/news_admin.html',
		insertAtBeginning: true,
		loginDeferred: App.Login.isLoggedDeferred,
		adminAccess: true
	});
	
});
