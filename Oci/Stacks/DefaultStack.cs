using Pulumi;
using Pulumi.Oci.Core;

namespace Oci.Stacks;

public class DefaultStack : Stack
{
    public DefaultStack()
    {
        var config = new Config();
        var compartmentOcid = config.Require("compartmentOcid");
        var vcnCidrBlock = config.Require("vcnCidrBlock");
        var subnetCidrBlock = config.Require("subnetCidrBlock");
        var ubuntu24ImageOcid = config.Require("ubuntu24ImageOcid");

        var vcn = new Vcn(
            "temasek3-prod-vcn",
            new VcnArgs
            {
                CompartmentId = compartmentOcid,
                CidrBlock = vcnCidrBlock,
                DisplayName = "temasek3-prod-vcn",
            }
        );

        var internetGateway = new InternetGateway(
            "temasek3-prod-igw",
            new InternetGatewayArgs
            {
                CompartmentId = compartmentOcid,
                VcnId = vcn.Id,
                DisplayName = "temasek3-prod-igw",
            }
        );

        var routeTable = new RouteTable(
            "temasek3-prod-rt",
            new RouteTableArgs
            {
                CompartmentId = compartmentOcid,
                VcnId = vcn.Id,
                DisplayName = "temasek3-prod-rt",
                RouteRules =
                {
                    new Pulumi.Oci.Core.Inputs.RouteTableRouteRuleArgs
                    {
                        Destination = "0.0.0.0/0",
                        DestinationType = "CIDR_BLOCK",
                        NetworkEntityId = internetGateway.Id,
                    },
                },
            }
        );

        var securityList = new SecurityList(
            "temasek3-prod-sl",
            new SecurityListArgs
            {
                CompartmentId = compartmentOcid,
                VcnId = vcn.Id,
                DisplayName = "temasek3-prod-sl",
                EgressSecurityRules =
                {
                    new Pulumi.Oci.Core.Inputs.SecurityListEgressSecurityRuleArgs
                    {
                        Protocol = "all",
                        Destination = "0.0.0.0/0",
                    },
                },
                IngressSecurityRules =
                {
                    new Pulumi.Oci.Core.Inputs.SecurityListIngressSecurityRuleArgs
                    {
                        Protocol = "all",
                        Source = "0.0.0.0/0",
                    },
                },
            }
        );

        var subnet = new Subnet(
            "temasek3-prod-subnet",
            new SubnetArgs
            {
                CompartmentId = compartmentOcid,
                VcnId = vcn.Id,
                CidrBlock = subnetCidrBlock,
                DisplayName = "temasek3-prod-subnet",
                RouteTableId = routeTable.Id,
                SecurityListIds = { securityList.Id },
            }
        );

        var instance = new Instance(
            "temasek3-prod",
            new InstanceArgs
            {
                CompartmentId = compartmentOcid,
                AvailabilityDomain = "YAbW:AP-SINGAPORE-1-AD-1",
                DisplayName = "temasek3-prod",
                Shape = "VM.Standard.A1.Flex",

                ShapeConfig = new Pulumi.Oci.Core.Inputs.InstanceShapeConfigArgs
                {
                    Ocpus = 1,
                    MemoryInGbs = 2,
                },

                CreateVnicDetails = new Pulumi.Oci.Core.Inputs.InstanceCreateVnicDetailsArgs
                {
                    SubnetId = subnet.Id,
                    AssignPublicIp = "true",
                },

                SourceDetails = new Pulumi.Oci.Core.Inputs.InstanceSourceDetailsArgs
                {
                    SourceType = "image",
                    SourceId = ubuntu24ImageOcid,
                    BootVolumeSizeInGbs = "50",
                },

                Metadata = new InputMap<string>
                {
                    {
                        "ssh_authorized_keys",
                        "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAICntO5DCRkXCmf5XYICVJMm1hYX/0OrVG++ZZPrJGHCQ qinguan@qins-legion"
                    },
                },

                FreeformTags = { { "managed-by", "pulumi" } },
            }
        );
    }
}
