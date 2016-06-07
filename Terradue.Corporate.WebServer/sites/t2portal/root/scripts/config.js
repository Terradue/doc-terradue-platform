
define({
	
	template: 'mustache',
	api: "t2api",
	errorNotify: true,
	errorLog: false,
	mainContainer: '#mainContainer',
	
	firstPage: '/static/home.html',

	pageError: 'modules/pages/views/error.html',
	
	staticPages: {
		about: '/static/about.html',
		partners: '/static/partners.html',
		tour: '/static/tour.html',
		resources: '/static/resources.html',
		media: '/static/media.html',
		terms_of_service: '/static/terms_of_service.html',
		terms_of_use: '/static/terms_of_use.html',
		privacy: '/static/privacy.html',
		security: '/static/security.html'
	},
	
	recaptchaPublicKey: '6Lc1ZgMTAAAAAFB21z0ElV23MU1friFPmkBXTtNc',
		//'6LeXcwMTAAAAAFoiZaGEqfI2WwzSDcXQVfm64H0J', // ciccio test
	
	baseControl: {
		siteName: 'Terradue',
		contactUsUrl: 'mailto:info@terradue.com',
		//errorImageUrl: '/styles/img/earthDeflated.jpg'
	}	

});