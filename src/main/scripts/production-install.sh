#! /bin/sh

#configuration
portal_path="/usr/local/t2portal/webserver"
site="t2portal"
db="t2portal"
dump="/tmp/t2portal-db-dump.sql"
dbuser="root"
publicIP="https://www.terradue.com"

mkdir -p $portal_path/services
mkdir -p $portal_path/modules

mkdir /var/www/.config
chown apache:apache /var/www/.config

mkdir $portal_path/sites/$site/root/logs
chown apache:apache $portal_path/sites/$site/root/logs

#link services
mkdir -p $portal_path/sites/$site/root/services
mkdir -p $portal_path/sites/$site/root/files
chown apache:apache $portal_path/sites/$site/root/files

#dynamic hostname
sed -i -e 's/${PORTALWEBSERVER}/'$HOSTNAME'/g' $portal_path/sites/$site/root/web.config
sed -i -e 's/${PORTALWEBSERVER}/'$HOSTNAME'/g' $portal_path/sites/$site/config/*
sed -i -e 's/${PORTALWEBSERVER}/'$HOSTNAME'/g' /etc/httpd/conf.d/*t2portal*

#dumped database copy
if [ -f $dump ] 
then 
    echo "Copy of the dumped database \`$db\`"
	mysql -u root -e "drop database if exists \`$db\`"
	mysql -u root -e "create database \`$db\`"
	mysql -u root -e "grant all on \`$db\`.* to 'portal'@'localhost' IDENTIFIED by 'portal'"
	mysql -u root $db < $dump
else
	echo "No copy of the dumped database $db"
	mysql -u root -e "grant all on \`$db\`.* to 'portal'@'localhost' IDENTIFIED by 'portal'" 
fi

#mono and mysql
mono $portal_path/sites/$site/root/bin/Terradue.Portal.AdminTool.exe auto -r $portal_path/sites/$site/root -u root -p romho1l@T2 -S $db

chkconfig --add t2portal-agent

