
define({
	template: 'mustache',
	api: "t2api",
	errorNotify: true,
	errorLog: false,
	mainContainer: '#mainContainer',
	
	firstPage: '/static/home.html',

	page404: "modules/pages/views/404.html",
	
	staticPages: {
		about: '/static/about.html',
		partners: '/static/partners.html',
		tour: '/static/tour.html',
		resources: '/static/resources.html',

	},
	
});