

define({
	template: 'mustache',
	errorNotify: true,
	errorLog: false,
	mainContainer: '#mainContainer',
	page404: "modules/pages/views/404.html",
	firstPage: '/static/home.html',
	api: "t2api",
	
	staticPages: {
		about: '/static/about.html',
		people: '/static/people.html',
		partners: '/static/partners.html',
		tour: '/static/tour.html',
		resources: '/static/resources.html',
		
		samplePeople: {
			url: '/static/sample.html',
			data: [
				{id:1, name: 'Kate', surname: 'Upton', image: 'http://images.ak.instagram.com/profiles/profile_1085312802_75sq_1394674277.jpg'},
				{id:2, name: 'Charlize', surname: 'Theron', image: 'http://img3.rnkr-static.com/list_img/2434/102434/150/charlize-theron-movies-and-films-and-filmography-u4.jpg'},
				{id:3, name: 'Alana', surname: 'Blanchard', image: 'http://coolspotters.com/files/photos/230994/alana-blanchard-large.jpg'},
				{id:4, name: 'Sophia', surname: 'Loren', image: 'http://www.milanolifestyle.it/wp-content/uploads/2014/09/sophia-loren-150x150.jpg'},
			],
			fnLoad: function(){
				// eventual javascript stuff
			}
		}
	},
	
});