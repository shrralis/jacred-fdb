#!/bin/bash
DEST="/home/jacred"
mkdir $DEST -p

# Become root
# sudo su -
apt update && apt install -y wget unzip cron

# Install .NET
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && chmod 755 dotnet-install.sh
./dotnet-install.sh --channel 6.0.1xx
mv $HOME/.dotnet $DEST/
echo "export DOTNET_ROOT=\$DEST/.dotnet" >> ~/.bashrc
echo "export PATH=\$PATH:\$DEST/.dotnet:\$DEST/.dotnet/tools" >> ~/.bashrc
source ~/.bashrc

# Download zip
cd $DEST
wget https://github.com/immisterio/jacred-fdb/releases/latest/download/publish.zip
unzip -o publish.zip
rm -f publish.zip

#systemctl enable cron

#crontab -l | { cat; echo "*/40 * * * * curl -s \"http://127.0.0.1:9117/jsondb/save\""; } | crontab -

# iptables drop
cat <<EOF > iptables-drop.sh
#!/bin/sh
echo "Stopping firewall and allowing everyone..."
iptables -F
iptables -X
iptables -t nat -F
iptables -t nat -X
iptables -t mangle -F
iptables -t mangle -X
iptables -P INPUT ACCEPT
iptables -P FORWARD ACCEPT
iptables -P OUTPUT ACCEPT
EOF

# Note
echo ""
echo "################################################################"
echo ""
echo "Have fun!"
echo ""
echo "Please check/edit $DEST/init.conf params and configure it"
echo ""
echo "Clear iptables if port 9117 is not available"
echo "bash $DEST/iptables-drop.sh"
echo ""
echo "Full setup crontab"
echo "crontab $DEST/Data/crontab"
echo ""
