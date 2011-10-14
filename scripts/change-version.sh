FROM=$1
TO=$2
OSGI_VERSION_TO=${TO/-/.}
OSGI_VERSION_FROM=${FROM/-/.}
if [ "$1" == "" ] 
then 
	echo "sh change-version.sh <from> <to>"
elif [ "$2" == "" ]
then
        echo "sh change-version.sh <from> <to>"
else 
		echo "will change from $1 to $2"
		find . -name pom.xml | xargs perl -i -pe "s/$1/$2/"
        find . -name build.xml | xargs perl -i -pe "s/$1/$2/"
        find . -name ra.xml | xargs perl -i -pe "s/$1/$2/"
        find . -name MANIFEST.MF | xargs perl -i -pe "s/$OSGI_VERSION_FROM/$OSGI_VERSION_TO/"
fi

