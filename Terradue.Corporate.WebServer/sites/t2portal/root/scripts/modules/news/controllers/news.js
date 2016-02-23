	
define(['jquery',
	'can',
	'config', 
	'utils/baseControl',
	'modules/news/models/news', 
	'utils/helpers'
], function($, can, Config, BaseControl, NewsModel, Helpers){

var NewsControl = BaseControl({}, {
	
	init: function(element, options){
		var self = this;
		console.log("newsControl.init");
		
		// setup data
		this.data = new can.Observe({
			q: null,
			page: 1,
			count: 3,
			pageOffset: 1,
			totalResults: 0,
			newses: null,
			news: null,
			loading: false
		});
		
		// load tags
		NewsModel.getTags().then(function(tags){
			self.data.attr('tags', tags.filter(function(tag){return (tag.Value>1)}));
		});
	},
	
	index: function(){
		console.log("App.controllers.News.index");
		var self = this;
		
		// reset q and page
		this.data.attr({ q: null, page: 1 });
		this.view({
			url: 'modules/news/views/newses.html',
			data: this.data
		});

		this.doSearch();
	},
	
	tag: function(routeData){
		console.log("App.controllers.News.tag");
		var self = this;
		var tag = routeData.id;
		
		// reset q and page
		this.data.attr({ q: tag, page: 1 });
		this.view({
			url: 'modules/news/views/newses.html',
			data: this.data
		});

		this.doSearch();
	},
	
	details: function(routeData){
		var self = this;
		var id = routeData.id;
		
		this.data.attr({
			loading: true,
			news: null
		});

		// load tags
		NewsModel.getTags().then(function(tags){
			self.data.attr('tags', tags);
		});

		this.view({
			url: 'modules/news/views/news.html',
			data: this.data
		});
		
		NewsModel.searchOne(id).then(function(newsResult){
			if (newsResult.features.length){
				var news = newsResult.features[0];
				self.addDataToNews(news);
				self.data.attr({
					loading: false,
					news: news
				});
			}
		});
	},
	
	doSearch: function(){
		var self = this;
		// "pluralized" news word into newses ;)
		this.data.attr({
			loading: true,
			newses: null
		});
		NewsModel.search(this.data.q, this.data.page, this.data.count).then(function(newses){
			self.addDataToNewses(newses);
			self.data.attr({
				loading: false,
				newses: newses, totalResults: newses.properties.totalResults
			});
		});
	},
	
	addDataToNewses: function(newses){
		var self = this;
		$.each(newses.features, function(i, news){
			self.addDataToNews(news);
		});
	},
	
	addDataToNews: function(news){
		// get image TODO:improve with a property for image
		if (news.properties.content.startsWith('<img src="')){
			var imgUrl = news.properties.content.substring(10, 10+news.properties.content.substring(10).indexOf('"'));
			news.properties.imageUrl = imgUrl;
		}
		
		news.properties.tags = [];
		if (news.properties.categories.length){
			$.each(news.properties.categories, function(i, cat){
				news.properties.tags.push(cat['@label']);
			});
		}
	},
	
	'.paginationBox a.changePage click': function(el){
		var page = el.data('page');
		if (page){
			this.data.attr('page', page)
			this.doSearch();
		}
		return false;
	},
	
});

return new NewsControl(Config.mainContainer, {});
	
});
