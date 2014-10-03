
define(['jquery', 'can', 'configurations/news', 'modules/news/models/news'], function($, can, Config, NewsModel){
	
	
	var NewsControl = can.Control({
		init: function(element, options){
			console.log("newsControl.init");
		},
		
		index: function(data){
			console.log("App.controllers.News.index");
			var self = this;
			// "pluralize" news word into newses ;)
			NewsModel.findAll({}, function(newses){				
				self.element.html(can.view("modules/news/views/newses.html", newses));
				if (data && data.fnLoad)
					data.fnLoad(newses);
			});
		},
		
		details: function(data){
			var self = this;
			NewsModel.findOne({id: data.id}, function(news){				
				self.element.html(can.view("modules/news/views/news.html", news));
				if (data && data.fnLoad)
					data.fnLoad(news);
			});
		}
		
	});

	return new NewsControl(Config.mainContainer, {});
	
});
